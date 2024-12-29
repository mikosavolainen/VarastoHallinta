const express = require('express');
const bodyParser = require('body-parser');
const mariadb = require('mariadb');
const cors = require('cors');

const app = express();
const port = 3000;

// MariaDB-yhteys
const pool = mariadb.createPool({
    host: 'localhost',
    user: 'root',
    password: '',
    database: 'warehouse',
    connectionLimit: 5
});

// Middleware
app.use(cors());
app.use(bodyParser.json());

// Muunna JSON-avainkentät pieniksi alkukirjaimiksi
app.use((req, res, next) => {
    if (req.body && typeof req.body === 'object') {
        req.body = Object.fromEntries(
            Object.entries(req.body).map(([key, value]) => [key.charAt(0).toLowerCase() + key.slice(1), value])
        );
    }
    next();
});

// Hae tuotteet
app.get('/products', async (req, res) => {
    try {
        const conn = await pool.getConnection();
        const rows = await conn.query('SELECT id, name, price, quantity FROM products');
        conn.release();
        res.json(rows);
    } catch (err) {
        res.status(500).send('Virhe tuotteiden haussa.');
    }
});

app.post('/purchase', async (req, res) => {
    const { customerName, code, items } = req.body;
    console.log("Vastaanotettu JSON:", req.body); // Debug-tulostus

    // Tarkistetaan syöte
    if (!customerName || !code || !Array.isArray(items) || items.length === 0) {
        return res.status(400).send('Virheellinen syöte: Tarkista asiakkaan nimi, kutsutunnus ja ostokset.');
    }

    let conn;
    try {
        conn = await pool.getConnection();
        await conn.beginTransaction(); // Aloitetaan transaktio

        let totalPrice = 0;
        let purchasedItems = [];

        for (const item of items) {
            const { productId, quantity } = item;

            // Tarkistetaan, että tuote ja määrä ovat kelvollisia
            if (!productId || !quantity || quantity <= 0) {
                throw new Error(`Virheellinen tuote tai määrä: ${JSON.stringify(item)}`);
            }

            // Haetaan tuote tietokannasta
            const [productRows] = await conn.query('SELECT id, name, price, quantity FROM products WHERE id = ?', [productId]);
            if (productRows.length === 0) {
                throw new Error(`Tuotetta ID:llä ${productId} ei löytynyt.`);
            }

            const product = productRows[0];
            if (product.quantity < quantity) {
                throw new Error(`Tuote ID ${productId}: Varasto ei riitä (${product.quantity} jäljellä).`);
            }

            // Päivitetään varaston saldo
            await conn.query('UPDATE products SET quantity = quantity - ? WHERE id = ?', [quantity, productId]);

            totalPrice += product.price * quantity; // Lasketaan kokonaishinta
            purchasedItems.push(`${product.name} (x${quantity})`); // Lisätään ostetut tuotteet
        }

        // Tallennetaan ostos tietokantaan
        const purchasedItemsStr = purchasedItems.join(", ");
        await conn.query(
            'INSERT INTO purchases (customer_name, customer_code, purchased_items, total_price) VALUES (?, ?, ?, ?)',
            [customerName, code, purchasedItemsStr, totalPrice]
        );

        await conn.commit(); // Suoritetaan transaktio
        conn.release(); // Vapautetaan yhteys

        res.send('Osto onnistui!'); // Palautetaan onnistumisviesti
    } catch (err) {
        console.error(err); // Tulostetaan virhe konsoliin

        // Tarkistetaan, onko conn määritelty ennen rollbackia
        if (conn) {
            await conn.rollback(); // Perutaan transaktio, jos virhe
            conn.release(); // Vapautetaan yhteys
        }

        res.status(500).send(`Virhe ostoksessa: ${err.message}`); // Virheilmoitus asiakkaalle
    }
});

// Käynnistä palvelin
app.listen(port, () => {
    console.log(`Palvelin käynnissä: http://localhost:${port}`);
});
