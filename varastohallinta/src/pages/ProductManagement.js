import React, { useState, useEffect } from "react";
import axios from "axios";
import {
  Container,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Button,
  TextField,
  Typography,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Snackbar,
  Paper,
} from "@mui/material";

const ProductManagement = () => {
  const [products, setProducts] = useState([]);
  const [cart, setCart] = useState([]);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [quantity, setQuantity] = useState("");
  const [openDialog, setOpenDialog] = useState(false);
  const [snackbarMessage, setSnackbarMessage] = useState("");
  const [userName, setUserName] = useState(""); // Käyttäjän nimi
  const [callSign, setCallSign] = useState(""); // Kutsutunnus

  const API_BASE_URL = "https://oh3acvarasto.oh3cyt.com";

  // Fetch products from API
  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const response = await axios.get(`${API_BASE_URL}/products`);
        setProducts(response.data);
      } catch (error) {
        console.error("Error fetching products:", error);
        setSnackbarMessage("Virhe tuotteiden lataamisessa.");
      }
    };

    fetchProducts();
  }, []);

  // Add product to cart
  const handleAddToCart = (product) => {
    setSelectedProduct(product);
    setOpenDialog(true);
  };

  const handleDialogClose = () => {
    setOpenDialog(false);
    setQuantity("");
  };

  const handleAddQuantity = () => {
    const qty = parseInt(quantity, 10);
    if (isNaN(qty) || qty <= 0 || qty > selectedProduct.quantity) {
      setSnackbarMessage("Virheellinen määrä.");
      return;
    }

    const existingItem = cart.find((item) => item.productId === selectedProduct.id);
    if (existingItem) {
      existingItem.quantity += qty;
      setCart([...cart]);
    } else {
      setCart([
        ...cart,
        {
          productId: selectedProduct.id,
          productName: selectedProduct.name,
          unitPrice: selectedProduct.price,
          quantity: qty,
        },
      ]);
    }

    handleDialogClose();
    setSnackbarMessage("Tuote lisätty ostoskoriin.");
  };

  // Handle purchase
  const handlePurchase = async () => {
    if (!userName || !callSign) {
      setSnackbarMessage("Nimi ja kutsutunnus ovat pakollisia.");
      return;
    }

    if (cart.length === 0) {
      setSnackbarMessage("Ostoskori on tyhjä.");
      return;
    }

    const purchase = {
      callsign: callSign,
      name: userName,
      purchased_items: cart.map((item) => ({
        id: item.productId,
        quantity: item.quantity,
      })),
      total_amount: cart.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0),
      date: new Date().toISOString(),
    };

    try {
      const response = await axios.post(`${API_BASE_URL}/purchase`, purchase);

      if (response.status === 200) {
        setSnackbarMessage("Osto onnistui!");
        setCart([]); // Tyhjennä ostoskori
        const refreshedProducts = await axios.get(`${API_BASE_URL}/products`);
        setProducts(refreshedProducts.data); // Päivitä tuotteet
      } else {
        setSnackbarMessage(`Virhe ostoa tehdessä: ${response.statusText}`);
      }
    } catch (error) {
      console.error("Error during purchase:", error);
      setSnackbarMessage("Virhe ostoa tehdessä.");
    }
  };

  // Calculate total price
  const totalPrice = cart.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);

  return (
    <Container>
      <Typography variant="h4" gutterBottom>
        Tuotehallinta
      </Typography>

      {/* Käyttäjän tiedot */}
      <TextField
        label="Nimi"
        value={userName}
        onChange={(e) => setUserName(e.target.value)}
        fullWidth
        margin="normal"
      />
      <TextField
        label="Kutsutunnus"
        value={callSign}
        onChange={(e) => setCallSign(e.target.value)}
        fullWidth
        margin="normal"
      />

      {/* Product Table */}
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Tuote</TableCell>
              <TableCell>Hinta (€)</TableCell>
              <TableCell>Varastossa</TableCell>
              <TableCell>Toiminnot</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {products
              .filter((product) => product.quantity > 0) 
              .map((product) => (
                <TableRow key={product.id}>
                  <TableCell>{product.name}</TableCell>
                  <TableCell>{parseFloat(product.price).toFixed(2)}</TableCell>
                  <TableCell>{product.quantity}</TableCell>
                  <TableCell>
                    <Button
                      variant="contained"
                      color="primary"
                      onClick={() => handleAddToCart(product)}
                    >
                      Lisää ostoskoriin
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Cart Section */}
      <Typography variant="h5" gutterBottom style={{ marginTop: "2rem" }}>
        Ostoskori
      </Typography>
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Tuote</TableCell>
              <TableCell>Määrä</TableCell>
              <TableCell>Hinta (€)</TableCell>
              <TableCell>Yhteensä (€)</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {cart.map((item) => (
              <TableRow key={item.productId}>
                <TableCell>{item.productName}</TableCell>
                <TableCell>{item.quantity}</TableCell>
                <TableCell>{parseFloat(item.unitPriceprice).toFixed(2)}</TableCell>
                <TableCell>{(item.unitPrice * item.quantity).toFixed(2)}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Typography variant="h6" gutterBottom style={{ marginTop: "1rem" }}>
        Yhteensä (ALV 25.5%): {totalPrice.toFixed(2)} €
      </Typography>

      <Button
        variant="contained"
        color="secondary"
        onClick={handlePurchase}
        style={{ marginTop: "1rem" }}
      >
        Osta tuotteet
      </Button>

      {/* Quantity Dialog */}
      <Dialog open={openDialog} onClose={handleDialogClose}>
        <DialogTitle>Lisää ostoskoriin</DialogTitle>
        <DialogContent>
          <Typography>
            Tuote: {selectedProduct?.name} (Varastossa: {selectedProduct?.quantity})
          </Typography>
          <TextField
            label="Määrä"
            type="number"
            value={quantity}
            onChange={(e) => setQuantity(e.target.value)}
            fullWidth
            margin="dense"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleDialogClose} color="secondary">
            Peruuta
          </Button>
          <Button onClick={handleAddQuantity} color="primary">
            Lisää
          </Button>
        </DialogActions>
      </Dialog>

      {/* Snackbar for messages */}
      <Snackbar
        open={!!snackbarMessage}
        autoHideDuration={3000}
        onClose={() => setSnackbarMessage("")}
        message={snackbarMessage}
      />
    </Container>
  );
};

export default ProductManagement;
