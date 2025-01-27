const express = require("express");
const bodyParser = require("body-parser");
const mariadb = require("mariadb");
const cors = require("cors");
const PDFDocument = require("pdfkit");
const fs = require("fs");
const axios = require("axios");
const FormData = require("form-data");

const app = express();
const port = 2058;

const pool = mariadb.createPool({
  host: "localhost",
  user: "root",
  password: "1234592",
  database: "warehouse",
  connectionLimit: 5,
});

// Discord Webhook URL
const DISCORD_WEBHOOK_URL =
  "https://discord.com/api/webhooks/1333356865136099338/EvQlBkT9itBunyT_yjyRxM_BSvvsty-UbZK2e9fBI_sBUGB6J3PDRL6vDe46tKDdMGNx";

// Asetetaan DEBUG-tilan arvo (true / false)
const DEBUG = true;

app.use(cors());
app.use(bodyParser.json());

app.use((req, res, next) => {
  if (req.body && typeof req.body === "object") {
    req.body = Object.fromEntries(
      Object.entries(req.body).map(([key, value]) => [
        key.charAt(0).toLowerCase() + key.slice(1),
        value,
      ])
    );
  }
  next();
});

// Status
app.get("/", async (req, res) => {
  try {
    res.status(200).send("Palvelin ja Domain Toimii");
  } catch (err) {
    if (DEBUG) console.error("Error in status route:", err);
    res.status(500).send("Virhe.");
  }
});

// Hae tuotteet
app.get("/products", async (req, res) => {
  try {
    const conn = await pool.getConnection();
    const rows = await conn.query(
      "SELECT id, name, price, quantity FROM products"
    );
    conn.release();
    res.json(rows);
  } catch (err) {
    if (DEBUG) console.error("Error fetching products:", err);
    res.status(500).send("Virhe tuotteiden haussa.");
  }
});

// Lisää tuote
app.post("/addProduct", async (req, res) => {
  const { name, price, quantity } = req.body;

  if (!name || !price || !quantity) {
    return res.status(400).json({ error: "Tietoja puuttuu" });
  }

  let conn;
  try {
    conn = await pool.getConnection();
    const existingProduct = await conn.query(
      "SELECT * FROM products WHERE name = ?",
      [name]
    );

    if (existingProduct.length > 0) {
      return res.status(409).json({ error: "Tuote on jo" });
    }

    await conn.query(
      "INSERT INTO products (name, price, quantity) VALUES (?, ?, ?)",
      [name, price, quantity]
    );
    res.status(201).json({ message: "Tuote lisätty onnistuneesti" });
  } catch (err) {
    if (DEBUG) console.error("Error adding product:", err);
    res.status(500).json({ error: "VIRHE Tuotetta lisätessä" });
  } finally {
    if (conn) conn.release();
  }
});

app.put("/updateProduct/:id", async (req, res) => {
  const productId = req.params.id;
  const { quantity } = req.body;

  if (!quantity || quantity < 0) {
    return res.status(400).json({ error: "Kelvollinen määrä on pakollinen." });
  }

  let conn;
  try {
    conn = await pool.getConnection();

    const product = await conn.query("SELECT * FROM products WHERE id = ?", [
      productId,
    ]);

    if (product.length === 0) {
      return res.status(404).json({ error: "Tuotetta ei löytynyt." });
    }

    await conn.query("UPDATE products SET quantity = ? WHERE id = ?", [
      quantity,
      productId,
    ]);

    res
      .status(200)
      .json({ message: "Tuotteen määrä päivitetty onnistuneesti." });
  } catch (err) {
    if (DEBUG) console.error("Error updating product:", err);
    res.status(500).json({ error: "Virhe tuotteen päivityksessä." });
  } finally {
    if (conn) conn.release();
  }
});

// Funktio tuotteiden määrän vähentämiseen
async function deductProductQuantities(conn, purchasedItems) {
  for (const item of purchasedItems) {
    const { id: productId, quantity } = item;

    const updateResult = await conn.query(
      "UPDATE products SET quantity = quantity - ? WHERE id = ? AND quantity >= ?",
      [quantity, productId, quantity]
    );

    if (updateResult.affectedRows === 0) {
      throw new Error(`Not enough stock for product ID ${productId}`);
    }
  }
}

// Funktio PDF:n luomiseen
function generateReceiptPDF(purchaseDetails) {
  const doc = new PDFDocument();
  const filePath = `./receipts/receipt_${purchaseDetails.purchaseId}.pdf`;

  doc.pipe(fs.createWriteStream(filePath));

  doc.fontSize(18).text("Receipt", { align: "center" });
  doc.moveDown();
  doc.fontSize(14).text(`Purchase Date: ${purchaseDetails.date}`);
  doc.text(`Buyer Name: ${purchaseDetails.name}`);
  doc.text(`Callsign: ${purchaseDetails.callsign}`);
  doc.moveDown();

  doc.text("Purchased Items:", { underline: true });
  purchaseDetails.items.forEach((item) => {
    doc.text(
      `${item.productName} - ${item.quantity} x ${item.price} = ${item.totalPrice}`
    );
  });

  doc.moveDown();
  doc.text(`Total Amount: ${purchaseDetails.totalAmount}`);
  doc.end();

  return filePath;
}

// Funktio lähettää PDF Discordiin
async function sendReceiptToDiscord(pdfPath) {
  try {
    const file = fs.createReadStream(pdfPath);

    const formData = new FormData();
    formData.append("file", file, { filename: "receipt.pdf" });

    await axios.post(DISCORD_WEBHOOK_URL, formData, {
      headers: formData.getHeaders(),
    });
  } catch (error) {
    if (DEBUG) console.error("Error sending receipt to Discord:", error);
  }
}

// Osto endpoint
app.post("/purchase", async (req, res) => {
  const { callsign, name, purchased_items, total_amount, date } = req.body;

  if (!callsign || !name || !purchased_items || !total_amount || !date) {
    return res.status(400).json({ error: "Puuttuvat pakolliset kentät" });
  }

  let conn;
  try {
    conn = await pool.getConnection();
    await conn.beginTransaction();

    const result = await conn.query(
      "INSERT INTO purchases (callsign, name, total_amount, date) VALUES (?, ?, ?, ?)",
      [callsign, name, total_amount, date]
    );
    const purchaseId = result.insertId;

    await deductProductQuantities(conn, purchased_items);

    for (const item of purchased_items) {
      const { id: productId, quantity } = item;
      await conn.query(
        "INSERT INTO purchase_items (purchase_id, product_id, quantity) VALUES (?, ?, ?)",
        [purchaseId, productId, quantity]
      );
    }

    await conn.commit();

    const purchaseDetails = {
      purchaseId,
      callsign,
      name,
      totalAmount: total_amount,
      date,
      items: purchased_items.map(item => ({
        productName: item.name,
        quantity: item.quantity,
        price: item.price,
        totalPrice: item.quantity * item.price,
      }))
    };

    const pdfPath = generateReceiptPDF(purchaseDetails);
    await sendReceiptToDiscord(pdfPath);

    res.status(200).json({ message: "Ostettu onnistuneesti" });
  } catch (err) {
    if (DEBUG) console.error("Error during purchase:", err);
    if (conn) await conn.rollback();
    res.status(400).json({ error: err.message });
  } finally {
    if (conn) conn.release();
  }
});

app.listen(port, () => {
  console.log(`Palvelin käynnissä: http://localhost:${port}`);
});
