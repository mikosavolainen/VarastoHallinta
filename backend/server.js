const express = require('express');
const bodyParser = require('body-parser');
const mariadb = require('mariadb');
const cors = require('cors');

const app = express();
const port = 3000;


const pool = mariadb.createPool({
    host: 'localhost',
    user: 'root',
    password: '',
    database: 'warehouse',
    connectionLimit: 5
});

app.use(cors());
app.use(bodyParser.json());


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

    if (!customerName || !code || !items || !Array.isArray(items)) {
        return res.status(400).send('Virheellinen syöte.');
    }

    try {
        const conn = await pool.getConnection();

        await conn.beginTransaction();

        let totalPrice = 0;
        let purchasedItems = [];

        for (const item of items) {
            const { productId, quantity } = item;

          
            const [product] = await conn.query('SELECT id, price, quantity FROM products WHERE id = ?', [productId]);
            if (!product || product.quantity < quantity) {
                throw new Error(`Tuote ${productId} ei ole saatavilla.`);
            }

            
            await conn.query('UPDATE products SET quantity = quantity - ? WHERE id = ?', [quantity, productId]);

           
            totalPrice += product.price * quantity;

            
            purchasedItems.push(`${product.name} (x${quantity})`);
        }

       
        const purchasedItemsStr = purchasedItems.join(", ");
        const query = `
            INSERT INTO purchases (customer_name, customer_code, purchased_items, total_price)
            VALUES (?, ?, ?, ?)
        `;
        
        await conn.query(query, [customerName, code, purchasedItemsStr, totalPrice]);

        await conn.commit();
        conn.release();

        res.send('Osto onnistui!');
    } catch (err) {
        res.status(500).send(`Virhe ostoksessa: ${err.message}`);
    }
});

app.listen(port, () => {
    console.log(`Palvelin käynnissä: http://localhost:${port}`);
});