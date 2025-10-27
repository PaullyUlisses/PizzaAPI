-- Pizza API Database Schema
-- This script initializes the database with the required tables

USE PizzaDB;

-- Create CUSTOMER table
CREATE TABLE IF NOT EXISTS CUSTOMER (
    id INT AUTO_INCREMENT PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    order_amount INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create PIZZA_BASE table
CREATE TABLE IF NOT EXISTS PIZZA_BASE (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create PIZZA_INGREDIENT table
CREATE TABLE IF NOT EXISTS PIZZA_INGREDIENT (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create PIZZA table
CREATE TABLE IF NOT EXISTS PIZZA (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    pizza_base_id INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (pizza_base_id) REFERENCES PIZZA_BASE(id) ON DELETE SET NULL
);

-- Create PIZZA_PIZZA_INGREDIENT junction table (many-to-many)
CREATE TABLE IF NOT EXISTS PIZZA_PIZZA_INGREDIENT (
    pizza_id INT,
    ingredient_id INT,
    PRIMARY KEY (pizza_id, ingredient_id),
    FOREIGN KEY (pizza_id) REFERENCES PIZZA(id) ON DELETE CASCADE,
    FOREIGN KEY (ingredient_id) REFERENCES PIZZA_INGREDIENT(id) ON DELETE CASCADE
);

-- Create ORDER table
CREATE TABLE IF NOT EXISTS `ORDER` (
    id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) DEFAULT 'pending',
    total_price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    FOREIGN KEY (customer_id) REFERENCES CUSTOMER(id) ON DELETE CASCADE
);

-- Create ORDER_ITEM table
CREATE TABLE IF NOT EXISTS ORDER_ITEM (
    id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT,
    pizza_id INT,
    quantity INT NOT NULL DEFAULT 1,
    price DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES `ORDER`(id) ON DELETE CASCADE,
    FOREIGN KEY (pizza_id) REFERENCES PIZZA(id) ON DELETE CASCADE
);

-- Insert sample data for testing
INSERT INTO PIZZA_BASE (name) VALUES 
('Thin Crust'),
('Thick Crust'),
('Stuffed Crust');

INSERT INTO PIZZA_INGREDIENT (name) VALUES 
('Mozzarella'),
('Pepperoni'),
('Mushrooms'),
('Bell Peppers'),
('Olives'),
('Tomato Sauce'),
('Basil');

INSERT INTO PIZZA (name, price, pizza_base_id) VALUES 
('Margherita', 12.99, 1),
('Pepperoni', 15.99, 1),
('Vegetarian', 14.99, 2);

-- Link pizzas with ingredients
INSERT INTO PIZZA_PIZZA_INGREDIENT (pizza_id, ingredient_id) VALUES 
(1, 1), (1, 6), (1, 7), -- Margherita: Mozzarella, Tomato Sauce, Basil
(2, 1), (2, 2), (2, 6), -- Pepperoni: Mozzarella, Pepperoni, Tomato Sauce
(3, 1), (3, 3), (3, 4), (3, 5), (3, 6); -- Vegetarian: Mozzarella, Mushrooms, Bell Peppers, Olives, Tomato Sauce

INSERT INTO CUSTOMER (first_name, last_name, email, password_hash) VALUES 
('John', 'Doe', 'john.doe@example.com', 'hashed_password_123'),
('Jane', 'Smith', 'jane.smith@example.com', 'hashed_password_456');