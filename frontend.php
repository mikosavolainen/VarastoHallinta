<!DOCTYPE html>
<html lang="fi">
<head>
    <meta charset="UTF-8">
    <title>OH3AC - Varastohallinta</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <style>
        /* Google Font */
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600&display=swap');

        body {
            font-family: 'Inter', sans-serif;
            margin: 2rem;
            background-color: #f4f4f4;
        }
        .container {
            max-width: 1000px;
            margin: auto;
            background: white;
            padding: 2rem;
            border-radius: 10px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }
        h1, h2 {
            color: #2c3e50;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 1rem;
        }
        th, td {
            padding: 0.5rem;
            border: 1px solid #ccc;
        }
        th {
            background-color: #eee;
        }
        .btn {
            background-color: #3498db;
            color: white;
            border: none;
            padding: 0.4rem 0.8rem;
            cursor: pointer;
            border-radius: 4px;
        }
        .btn:hover {
            background-color: #2980b9;
        }

        .section {
            margin-top: 2rem;
        }

        .form-row {
            display: flex;
            gap: 1rem;
            margin-bottom: 1rem;
            flex-wrap: wrap;
        }

        .form-group {
            flex: 1 1 calc(50% - 0.5rem);
        }

        input, select, textarea {
            width: 100%;
            padding: 0.4rem;
            margin-top: 0.2rem;
        }

        .cart-total {
            font-size: 1.2rem;
            font-weight: bold;
            margin-top: 1rem;
        }

        .info {
            color: gray;
        }
    </style>
</head>
<body>

<?php
$feedback = "";
if (isset($_GET['success'])) {
    $feedback = "<p style='color:green;'>Osto tehty onnistuneesti!</p>";
} elseif (isset($_GET['error'])) {
    $feedback = "<p style='color:red;'>Virhe ostosta: " . htmlspecialchars(urldecode($_GET['error'])) . "</p>";
}
?>

<div class="container">
    <h1>OH3AC - Varastohallinta</h1>
    <?= $feedback ?>

    <div class="section">
        <h2>Tuotteet</h2>
        <?php
        // Haetaan tuotteet API:sta
        $curl = curl_init("http://localhost:2058/products");
        curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);
        $response = curl_exec($curl);
        curl_close($curl);

        $products = json_decode($response, true);
        if (!is_array($products)) {
            $products = [];
        }

        // Kerätään kategoriat pudotusvalikkoa varten
        $categories = [];
        if ($products) {
            $categories = array_filter(array_unique(array_column($products, 'category')));
        }
        ?>
        <form method="POST">
            <input type="hidden" name="action" value="list_products">
            <button type="submit" class="btn">Hae tuotteet (Päivitä)</button>
        </form>
        <?php if (count($products) > 0): ?>
        <table>
            <thead>
                <tr>
                    <th>ID</th><th>Nimi</th><th>Hinta (€)</th><th>Määrä</th><th>Kategoria</th><th>Kuvaus</th><th>Toiminnot</th>
                </tr>
            </thead>
            <tbody>
                <?php foreach ($products as $product): ?>
                    <tr>
                        <td><?= htmlspecialchars($product['id']) ?></td>
                        <td><?= htmlspecialchars($product['name']) ?></td>
                        <td><?= number_format($product['price'], 2) ?></td>
                        <td><?= htmlspecialchars($product['quantity']) ?></td>
                        <td><?= htmlspecialchars($product['category'] ?? '-') ?></td>
                        <td><?= htmlspecialchars($product['desc'] ?? '-') ?></td>
                        <td><button type="button" onclick='addToCart(<?= json_encode($product) ?>)' class="btn">Lisää koriin</button></td>
                    </tr>
                <?php endforeach; ?>
            </tbody>
        </table>
        <?php else: ?>
            <p class="info">Ei tuotteita saatavilla.</p>
        <?php endif; ?>
    </div>

    <div class="section">
        <h2>Ostoskori</h2>
        <table id="cartTable" class="cart-table">
            <thead>
                <tr>
                    <th>ID</th><th>Nimi</th><th>Hinta (€)</th><th>Määrä</th><th>Yhteensä (€)</th>
                </tr>
            </thead>
            <tbody id="cartBody">
                <tr><td colspan="5">Kori on tyhjä.</td></tr>
            </tbody>
        </table>
        <div class="cart-total" id="cartTotal">
            Yhteensä: 0.00 €
        </div>

        <h2>Tee osto</h2>
        <form id="purchaseForm" method="POST">
            <input type="hidden" name="action" value="purchase">
            <div class="form-row">
                <div class="form-group">
                    <label for="callsign">Kutsutunnus *</label>
                    <input type="text" id="callsign" name="callsign" required>
                </div>
                <div class="form-group">
                    <label for="customer_name">Nimi *</label>
                    <input type="text" id="customer_name" name="name" required>
                </div>
                <div class="form-group">
                    <label for="date">Päivämäärä ja aika *</label>
                    <input type="datetime-local" id="date" name="date" required>
                </div>
                <div class="form-group">
                    <label for="total_amount">Yhteensä (€) *</label>
                    <input type="number" step="0.01" id="total_amount" name="total_amount" readonly required>
                </div>
            </div>
            <input type="hidden" id="purchased_items_input" name="purchased_items" value="">
            <button type="submit" class="btn">Tee osto</button>
        </form>
    </div>

    <div class="section">
        <h2>Lisää uusi tuote</h2>
        <form id="addProductForm" action="http://localhost:2058/addProduct" method="POST">
            <div class="form-row">
                <div class="form-group">
                    <label for="name">Nimi *</label>
                    <input type="text" id="name" name="name" required>
                </div>
                <div class="form-group">
                    <label for="price">Hinta (€) *</label>
                    <input type="number" step="0.01" id="price" name="price" required>
                </div>
                <div class="form-group">
                    <label for="quantity">Määrä *</label>
                    <input type="number" id="quantity" name="quantity" required>
                </div>
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label for="existing_category">Valitse kategoria</label>
                    <select id="existing_category" name="category">
                        <option value="">-- Valitse --</option>
                        <?php foreach ($categories as $cat): if ($cat): ?>
                            <option value="<?= htmlspecialchars($cat) ?>"><?= htmlspecialchars($cat) ?></option>
                        <?php endif; endforeach; ?>
                    </select>
                </div>
                <div class="form-group">
                    <label for="new_category">Tai kirjoita uusi kategoria</label>
                    <input type="text" id="new_category" name="new_category" placeholder="Uusi kategoria">
                </div>
            </div>
            <div class="form-group">
                <label for="desc">Kuvaus</label>
                <textarea id="desc" name="desc"></textarea>
            </div>
            <button type="submit" class="btn">Lisää tuote</button>
        </form>
    </div>
</div>

<script>
    let cart = [];

    function addToCart(product) {
        const existingItem = cart.find(item => item.id == product.id);
        if (existingItem) {
            alert("Tuote on jo korissa!");
            return;
        }

        product.quantity = 1;
        cart.push(product);
        updateCart();
    }

    function updateCart() {
        const tbody = document.getElementById('cartBody');
        const totalEl = document.getElementById('cartTotal');
        const totalInput = document.getElementById('total_amount');
        const itemsInput = document.getElementById('purchased_items_input');

        tbody.innerHTML = "";

        let total = 0;

        cart.forEach((item, index) => {
            const row = document.createElement("tr");
            row.innerHTML = `
                <td>${item.id}</td>
                <td>${item.name}</td>
                <td>${parseFloat(item.price).toFixed(2)}</td>
                <td><input type="number" value="${item.quantity}" min="1" onchange="updateQuantity(${index}, this.value)"></td>
                <td>${(item.price * item.quantity).toFixed(2)}</td>
            `;
            tbody.appendChild(row);
            total += item.price * item.quantity;
        });

        totalEl.textContent = "Yhteensä: " + total.toFixed(2) + " €";
        totalInput.value = total.toFixed(2);
        itemsInput.value = JSON.stringify(cart.map(item => ({ id: item.id, quantity: item.quantity })));
    }

    function updateQuantity(index, quantity) {
        cart[index].quantity = parseInt(quantity);
        updateCart();
    }

    document.getElementById('purchaseForm').addEventListener('submit', function(e) {
        if (cart.length === 0) {
            e.preventDefault();
            alert("Kori on tyhjä.");
        }
    });

    // Lomakkeen validointi ja uuden kategorian valinta
    document.getElementById("addProductForm").addEventListener("submit", function(e) {
        const newCat = document.getElementById("new_category").value.trim();
        const existingSelect = document.getElementById("existing_category");

        if (newCat) {
            const hiddenInput = document.createElement("input");
            hiddenInput.type = "hidden";
            hiddenInput.name = "category";
            hiddenInput.value = newCat;
            this.appendChild(hiddenInput);
        } else if (!existingSelect.value) {
            alert("Valitse tai kirjoita kategoria.");
            e.preventDefault();
        }
    });
</script>

</body>
</html>