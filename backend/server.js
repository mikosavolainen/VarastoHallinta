const express = require("express");
const bodyParser = require("body-parser");
const mariadb = require("mariadb");
const cors = require("cors");
const PDFDocument = require("pdfkit");
const fs = require("fs");
const axios = require("axios");
const FormData = require("form-data");

const multer = require("multer");
const upload = multer({ storage: multer.memoryStorage() });

const app = express();
const port = 2058;

// Yhteys tietokantaan
const pool = mariadb.createPool({
  host: "localhost",
  user: "root",
  password: "",
  database: "warehouse",
  connectionLimit: 5,
});

// Discord Webhook URL
const DISCORD_WEBHOOK_URL =
  "https://discord.com/api/webhooks/1366855317564166195/rQ1Hg4hDaTqkhVvbIGkUZiYFuqMtO4N3OCCXI53leR3ixQp-LjRKPsNqBYwZzwmqZeVu";

// DEBUG-tila
const DEBUG = true;

app.use(cors());
app.use(bodyParser.json());

// Kirjoitetaan bodyn avaimet lowercase-alkuisiksi (vakava tyylipiste)
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

// Status endpointti
app.get("/", async (req, res) => {
  try {
    res.status(200).send("Palvelin ja Domain Toimii");
  } catch (err) {
    if (DEBUG) console.error("Error in status route:", err);
    res.status(500).send("Virhe.");
  }
});

// Hae kaikki tuotteet
app.get("/products", async (req, res) => {
  try {
    const conn = await pool.getConnection();
    const rows = await conn.query(
      "SELECT `id`, `name`, `price`, `quantity`, `category`, `desc`, `vanha` FROM products"
    );
    conn.release();
    res.json(rows);
  } catch (err) {
    if (DEBUG) console.error("Error fetching products:", err);
    res.status(500).send("Virhe tuotteiden haussa.");
  }
});

const { Client, GatewayIntentBits } = require("discord.js");


const DISCORD_TOKEN = process.env.DISCORD_TOKEN || "";
const DISCORD_CHANNEL_ID = process.env.DISCORD_CHANNEL_ID || "";
const DISCORD_LOG_CHANNEL = process.env.DISCORD_LOG_CHANNEL || "";


const discordClient = new Client({
  intents: [GatewayIntentBits.Guilds, GatewayIntentBits.GuildMessages],
});

discordClient.once("ready", () => {
  console.log(`✅ Discord botti kirjautunut sisään: ${discordClient.user.tag}`);
});

discordClient.login(DISCORD_TOKEN);

// Lisää uusi tuote
app.post("/addProduct", upload.single("image"), async (req, res) => {
  const { name, price, quantity, category, desc } = req.body;

  if (!name || !price || !quantity) {
    return res.status(400).json({ error: "Pakollisia tietoja puuttuu" });
  }

  let conn;
  try {
    conn = await pool.getConnection();
    const existingProduct = await conn.query(
      "SELECT * FROM products WHERE name = ?",
      [name]
    );

    if (existingProduct.length > 0) {
      return res.status(409).json({ error: "Tuote on jo olemassa." });
    }

    let imageUrl = null;

    // Jos tiedosto mukana -> lähetetään Discordiin
    if (req.file) {
      const buffer = req.file.buffer;
      const channel = await discordClient.channels.fetch(DISCORD_CHANNEL_ID);
      if (!channel) throw new Error("Kuva-kanavaa ei löydy Discordista");

      const message = await channel.send({
        content: `📦 Uusi tuotekuva: ${name}`,
        files: [{ attachment: buffer, name: req.file.originalname }],
      });

      if (message.attachments.size > 0) {
        imageUrl = message.attachments.first().url;
      }
    }

    await conn.query(
      "INSERT INTO products (name, price, quantity, category, `desc`, image_url) VALUES (?, ?, ?, ?, ?, ?)",
      [name, price, quantity, category, desc, imageUrl]
    );

    // Lähetä logiin
    const logChannel = await discordClient.channels.fetch(DISCORD_LOG_CHANNEL);
    if (logChannel) {
      await logChannel.send(
        `🆕 Tuote lisätty: **${name}** (${price} €) – kuva: ${imageUrl || "Ei kuvaa"}`
      );
    }

    res.status(201).json({ message: "Tuote lisätty onnistuneesti", imageUrl });
  } catch (err) {
    if (DEBUG) console.error("Error adding product:", err);
    res.status(500).json({ error: "Virhe tuotetta lisättäessä" });
  } finally {
    if (conn) conn.release();
  }
});

// Päivitä tuote (kaikki kentät mahdollisia)
app.put("/updateProduct/:id", async (req, res) => {
  const productId = req.params.id;
  const updates = req.body;

  // Validoidaan että tuote on olemassa
  let conn;
  try {
    conn = await pool.getConnection();

    const product = await conn.query("SELECT * FROM products WHERE id = ?", [
      productId,
    ]);

    if (product.length === 0) {
      return res.status(404).json({ error: "Tuotetta ei löytynyt." });
    }

    const fields = [];
    const values = [];

    for (const field of ["name", "price", "quantity", "category", "desc"]) {
      if (updates[field] !== undefined) {
        fields.push(`${field} = ?`);
        values.push(updates[field]);
      }
    }

    if (fields.length === 0) {
      return res.status(400).json({ error: "Ei päivitettäviä kenttiä." });
    }

    values.push(productId);

    await conn.query(
      `UPDATE products SET ${fields.join(", ")} WHERE id = ?`,
      values
    );

    res.status(200).json({ message: "Tuotteen tieto päivitetty." });
  } catch (err) {
    if (DEBUG) console.error("Error updating product:", err);
    res.status(500).json({ error: "Virhe tuotteen päivityksessä." });
  } finally {
    if (conn) conn.release();
  }
});

// Funktio vähentää ostosten määrät
async function deductProductQuantities(conn, purchasedItems) {
  for (const item of purchasedItems) {
    const { id: productId, quantity } = item;

    const [result] = await conn.query(
      "UPDATE products SET quantity = quantity - ? WHERE id = ? AND quantity >= ?",
      [quantity, productId, quantity]
    );

    if (result.affectedRows === 0) {
      throw new Error(`Ei varastossa tarpeeksi tuotetta ID: ${productId}`);
    }
  }
}

// Laskun PDF:n generointi
function generateReceiptPDF(purchaseDetails) {
  const doc = new PDFDocument();
  const filePath = `./receipts/receipt_${purchaseDetails.purchaseId}.pdf`;

  doc.pipe(fs.createWriteStream(filePath));

  doc.fontSize(18).text("OSTOKSETTELO", { align: "center" });
  doc.moveDown();
  doc.text(`Päivämäärä: ${purchaseDetails.date}`);
  doc.text(`Nimi: ${purchaseDetails.name}`);
  doc.text(`Kutsutunnus: ${purchaseDetails.callsign}`);
  doc.moveDown();
  doc.text("Ostetut tuotteet:");

  purchaseDetails.items.forEach((item) => {
    doc.text(
      `${item.productName} – ${item.quantity} x ${item.price} € = ${(item.quantity * item.price).toFixed(2)} €`
    );
  });

  doc.moveDown();
  doc.text(`Yhteensä: ${purchaseDetails.totalAmount.toFixed(2)} €`);
  doc.end();

  return filePath;
}

// Lähetä PDF Discordiin
async function sendReceiptToDiscord(pdfPath) {
  try {
    const file = fs.createReadStream(pdfPath);
    const formData = new FormData();
    formData.append("file", file, { filename: "receipt.pdf" });

    await axios.post(DISCORD_WEBHOOK_URL, formData, {
      headers: formData.getHeaders(),
    });
  } catch (error) {
    if (DEBUG) console.error("Virhe lähettäessä PDF:ää Discordiin:", error);
  }
}

// Ostotapahtuma
app.post("/purchase", async (req, res) => {
  const { callsign, name, purchased_items, total_amount, date } = req.body;

  if (!callsign || !name || !purchased_items || !total_amount || !date) {
    return res.status(400).json({ error: "Puuttuvat kentät" });
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

    // Haetaan nimet jokaiselle tuotteelle
    const purchaseDetails = {
      purchaseId,
      callsign,
      name,
      totalAmount: total_amount,
      date,
      items: [],
    };

    for (const item of purchased_items) {
      const [product] = await conn.query(
        "SELECT name, price FROM products WHERE id = ?",
        [item.id]
      );
      purchaseDetails.items.push({
        productName: product.name,
        quantity: item.quantity,
        price: product.price,
        totalPrice: item.quantity * product.price,
      });
    }

    const pdfPath = generateReceiptPDF(purchaseDetails);
    await sendReceiptToDiscord(pdfPath);

    res
      .status(200)
      .json({ message: "Osto suoritettu onnistuneesti!", purchaseId });
  } catch (err) {
    if (conn) await conn.rollback();
    if (DEBUG) console.error("Ostossa virhe:", err);
    res.status(400).json({ error: err.message });
  } finally {
    if (conn) conn.release();
  }
});

app.listen(port, () => {
  console.log(`Palvelin käynnissä: http://localhost:${port}`);
});