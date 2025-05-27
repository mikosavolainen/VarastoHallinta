-- --------------------------------------------------------
-- Verkkotietokone:              127.0.0.1
-- Palvelinversio:               10.10.2-MariaDB - mariadb.org binary distribution
-- Server OS:                    Win64
-- HeidiSQL Versio:              11.3.0.6295
-- Skripti:                      Poistaa koko warehouse-tietokannan ja luo sen uudelleen
-- --------------------------------------------------------

-- 1. Poista vanha tietokanta (jos se on olemassa)
DROP DATABASE IF EXISTS `warehouse`;

-- 2. Luo uusi tyhjä tietokanta
CREATE DATABASE `warehouse` /*!40100 DEFAULT CHARACTER SET latin1 COLLATE latin1_swedish_ci */;

-- 3. Valitse käytettäväksi tietokanta
USE `warehouse`;

-- 4. Laitetaan väliaikaisesti pois tietyt asetukset turvallisuutta varten
/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- 5. Luodaan products-taulu
CREATE TABLE IF NOT EXISTS `products` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `quantity` int(11) NOT NULL,
  `category` varchar(50) DEFAULT NULL,
  `desc` varchar(500) DEFAULT NULL,
  `vanha` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

-- 6. Luodaan purchases-taulu
CREATE TABLE IF NOT EXISTS `purchases` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `callsign` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `total_amount` decimal(10,2) NOT NULL,
  `date` varchar(50) NOT NULL DEFAULT '',
  `items` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL CHECK (json_valid(`items`)),
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

-- 7. Lisätään tuotteet products-tauluun
INSERT INTO `products` (`id`, `name`, `price`, `quantity`, `category`, `desc`, `vanha`) VALUES
    (1, 'FT240-43', 9.75, 68, 'ferrite', NULL, 0),
    (2, 'FT240-61', 14.00, 50, 'ferrite', NULL, 0),
    (3, 'rg-214', 1.59, 498, 'kaapeli', NULL, 0),
    (4, 'FT-2423', 12.00, 799, 'ferrite', NULL, 0),
    (7, 'rg-58', 1.00, 400, 'kaapeli', NULL, 0),
    (8, 'FT-990', 399.00, 1, 'vanhat', 'Tästä toimiva ja hyvä Yaesu hyvään hintaan 100W ja sisäänrakennettulla virtalähteellä', 1),
    (9, 'RTL-SDR-HF', 22.00, 2, 'Kaapeli', '', 0);

-- 8. Lisätään ostot purchases-tauluun
INSERT INTO `purchases` (`id`, `callsign`, `name`, `total_amount`, `date`, `items`) VALUES
    (3, 'adad', 'sdas', 19.50, '2025-01-17T22.15.28Z', NULL),
    (4, '2easd3q2', 'AaaaAaA', 227.43, '2025-01-17T22.15.51Z', NULL),
    (5, 'asdada', 'asdasd', 19.50, '2025-01-17T22.31.43Z', NULL),
    (6, 'asdada', 'asdasd', 29.25, '2025-01-17T22.31.49Z', NULL),
    (7, 'oh3cyt', 'Miko', 19.50, '2025-01-17T22.35.36Z', NULL);

-- 9. Palautetaan alkuperäiset asetukset
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;