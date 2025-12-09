-- ========================================
-- Pizza API Database Schema
-- ========================================
-- This script initializes the database with the required tables for the Pizza API
-- with JWT authentication and role-based authorization
--
-- AUTHENTICATION SYSTEM:
--   - JWT-based authentication with 1-hour access tokens
--   - BCrypt password hashing (sample hashes provided below)
--   - Role-based authorization: Admin and User roles
--
-- ROLES:
--   - Admin: Full access to all resources (manage pizzas, orders, customers)
--   - User: Can view pizza menu, create own orders, view own profile
--
-- NOTES:
--   - This schema matches the Entity Framework Core Code First model
--   - Enums are stored as INT values (PizzaBase, PizzaIngredient, OrderStatus)
--   - The actual application uses EF migrations and DbSeeder.cs for data seeding
--   - For production, customer passwords should be properly hashed with BCrypt
--
-- LAST UPDATED: 2025-12-09
-- Added: role column to CUSTOMER table for role-based authorization
-- ========================================

USE PizzaDB;

-- Create CUSTOMER table
CREATE TABLE IF NOT EXISTS CUSTOMER (
    id INT AUTO_INCREMENT PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'User',
    order_amount DECIMAL(10,2) DEFAULT 0.00,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create PIZZA table
-- Note: base_id stores PizzaBase enum value (0=Tomato, 1=Cream, 2=Pesto, etc.)
CREATE TABLE IF NOT EXISTS PIZZA (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    base_id INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create PIZZA_INGREDIENT_MAPPING table
-- Note: ingredient_id stores PizzaIngredient enum value
-- (0=Mozzarella, 1=Basil, 2=Pepperoni, 3=Sausage, etc.)
CREATE TABLE IF NOT EXISTS PIZZA_INGREDIENT_MAPPING (
    id INT AUTO_INCREMENT PRIMARY KEY,
    pizza_id INT NOT NULL,
    ingredient_id INT NOT NULL,
    FOREIGN KEY (pizza_id) REFERENCES PIZZA(id) ON DELETE CASCADE
);

-- Create ORDER table
-- Note: status stores OrderStatus enum value (0=Pending, 1=Confirmed, 2=InProgress, 3=Ready, 4=Delivered, 5=Cancelled)
CREATE TABLE IF NOT EXISTS `ORDER` (
    id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT NOT NULL,
    orderDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status INT NOT NULL DEFAULT 0,
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
-- PizzaBase enum: 0=Tomato, 1=Cream, 2=Pesto, 3=White, 4=BBQ
-- PizzaIngredient enum: 0=Mozzarella, 1=Basil, 2=Pepperoni, 3=Sausage, 4=Ham, 5=Bacon,
--                       6=Chicken, 7=Mushrooms, 8=Onions, 9=Bell_Peppers, 10=Olives,
--                       11=Pineapple, 12=Spinach, 13=Tomatoes

INSERT INTO PIZZA (name, price, base_id) VALUES
('Margherita', 12.99, 0),        -- Tomato base
('Pepperoni', 14.99, 0),         -- Tomato base
('Hawaiian', 15.99, 1),          -- Cream base
('Meat Lovers', 18.99, 0),       -- Tomato base
('Veggie Supreme', 16.99, 0),    -- Tomato base
('BBQ Chicken', 17.99, 1);       -- Cream base

-- Link pizzas with ingredients
INSERT INTO PIZZA_INGREDIENT_MAPPING (pizza_id, ingredient_id) VALUES
-- Margherita: Mozzarella, Basil
(1, 0), (1, 1),
-- Pepperoni: Mozzarella, Pepperoni
(2, 0), (2, 2),
-- Hawaiian: Mozzarella, Ham, Pineapple
(3, 0), (3, 4), (3, 11),
-- Meat Lovers: Mozzarella, Pepperoni, Sausage, Ham, Bacon
(4, 0), (4, 2), (4, 3), (4, 4), (4, 5),
-- Veggie Supreme: Mozzarella, Mushrooms, Onions, Bell Peppers, Olives, Spinach
(5, 0), (5, 7), (5, 8), (5, 9), (5, 10), (5, 12),
-- BBQ Chicken: Mozzarella, Chicken, Onions
(6, 0), (6, 6), (6, 8);

-- Insert sample customers with BCrypt hashed passwords
-- Note: These passwords are BCrypt hashes for demonstration
-- IMPORTANT: In actual deployment, use the application's /api/auth/register endpoint
--            which will properly hash passwords with BCrypt
-- Test Accounts:
--   Admin: admin@pizzaapi.com / Admin123!
--   Users: john.doe@email.com, jane.smith@email.com, mike.johnson@email.com / Password123!
INSERT INTO CUSTOMER (first_name, last_name, email, password_hash, role, order_amount) VALUES
('Admin', 'User', 'admin@pizzaapi.com', '$2a$11$sampleBCryptHashForAdmin123', 'Admin', 0.00),
('John', 'Doe', 'john.doe@email.com', '$2a$11$sampleBCryptHashForPassword123', 'User', 0.00),
('Jane', 'Smith', 'jane.smith@email.com', '$2a$11$sampleBCryptHashForPassword123', 'User', 0.00),
('Mike', 'Johnson', 'mike.johnson@email.com', '$2a$11$sampleBCryptHashForPassword123', 'User', 0.00);

-- ========================================
-- ENUM VALUE REFERENCE
-- ========================================
-- Use these integer values when inserting data manually
--
-- PizzaBase Enum (stored in PIZZA.base_id):
--   0 = Tomato
--   1 = Cream
--   2 = Pesto
--   3 = White
--   4 = BBQ
--
-- PizzaIngredient Enum (stored in PIZZA_INGREDIENT_MAPPING.ingredient_id):
--   0 = Mozzarella      7 = Mushrooms
--   1 = Basil           8 = Onions
--   2 = Pepperoni       9 = Bell_Peppers
--   3 = Sausage        10 = Olives
--   4 = Ham            11 = Pineapple
--   5 = Bacon          12 = Spinach
--   6 = Chicken        13 = Tomatoes
--
-- OrderStatus Enum (stored in ORDER.status):
--   0 = Pending
--   1 = Confirmed
--   2 = InProgress
--   3 = Ready
--   4 = Delivered
--   5 = Cancelled
--
-- Customer Roles (stored in CUSTOMER.role):
--   'Admin' = Full administrative access
--   'User'  = Standard customer access (default)
-- ========================================