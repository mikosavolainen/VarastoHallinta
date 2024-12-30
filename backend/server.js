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

app.post('/purchase', (req, res) => {
    const { callsign, name, purchased_items, total_amount, date } = req.body;

    if (!callsign || !name || !purchased_items || !total_amount || !date) {
        return res.status(400).json({ error: 'Missing required fields' });
    }

    const connection = db.promise();

    connection.beginTransaction()
        .then(() => {
            // Lisää ostos purchases-tauluun
            return connection.query(
                'INSERT INTO purchases (callsign, name, total_amount, date) VALUES (?, ?, ?, ?)',
                [callsign, name, total_amount, date]
            );
        })
        .then(([result]) => {
            const purchaseId = result.insertId;

            const updateStockPromises = purchased_items.map(item => {
                const { id: productId, quantity } = item;

                return connection.query(
                    'UPDATE products SET quantity = quantity - ? WHERE id = ? AND quantity >= ?',
                    [quantity, productId, quantity]
                ).then(([result]) => {
                    if (result.affectedRows === 0) {
                        throw new Error(`Not enough stock for product ID ${productId}`);
                    }

                    return connection.query(
                        'INSERT INTO purchase_items (purchase_id, product_id, quantity) VALUES (?, ?, ?)',
                        [purchaseId, productId, quantity]
                    );
                });
            });

            return Promise.all(updateStockPromises);
        })
        .then(() => {
            return connection.commit();
        })
        .then(() => {
            res.status(200).json({ message: 'Purchase recorded successfully' });
        })
        .catch(err => {
            connection.rollback();
            res.status(400).json({ error: err.message });
        });
});



// Käynnistä palvelin
app.listen(port, () => {
    console.log(`Palvelin käynnissä: http://localhost:${port}`);
});
