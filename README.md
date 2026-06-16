# 🛒 E-Commerce Backend Database Architecture
The database architecture is designed to support:

* User Authentication & Authorization
* Product Catalog Management
* Product Variants & Inventory
* Shopping Cart & Wishlist
* Order Processing
* Payment Integration (Razorpay / COD)
* Order Tracking
* Product Reviews & Ratings
* Coupon Management
* Notifications
* AI Chat Support

# Database Modules

| Module         | Tables                                                        |
| -------------- | ------------------------------------------------------------- |
| Authentication | Users, Addresses, OtpRecords                                  |
| Catalog        | Categories, Products, ProductImages, ProductVariants, Reviews |
| Shopping       | WishlistItems, CartItems                                      |
| Orders         | Orders, OrderItems, TrackingEvents                            |
| Marketing      | Coupons, Notifications                                        |
| Support        | ChatMessages                                                  |

# Database Schema Structure

DATABASE
│
├── Users
│   ├── Addresses
│   ├── OtpRecords
│   ├── WishlistItems
│   ├── CartItems
│   ├── Orders
│   │   ├── OrderItems
│   │   └── TrackingEvents
│   ├── Reviews
│   ├── Notifications
│   └── ChatMessages
│
├── Categories
│   └── Products
│       ├── ProductImages
│       ├── ProductVariants
│       └── Reviews
│
└── Coupons

# Entity Relationship Diagram (ERD)

erDiagram

    USERS {
        int Id PK
        string Phone UK
        string Name
        string Email
        string Role
    }

    ADDRESSES {
        int Id PK
        int UserId FK
        string Label
        string City
        string State
        string Pincode
    }

    OTP_RECORDS {
        int Id PK
        string Phone
        string Code
        datetime ExpiresAt
    }

    CATEGORIES {
        int Id PK
        string Name
        string Slug UK
        int ParentId FK
    }

    PRODUCTS {
        int Id PK
        int CategoryId FK
        string Title
        string Brand
        decimal BasePrice
    }

    PRODUCT_IMAGES {
        int Id PK
        int ProductId FK
        string Url
    }

    PRODUCT_VARIANTS {
        int Id PK
        int ProductId FK
        string SKU UK
        string Color
        string Size
        decimal Price
        int Stock
    }

    REVIEWS {
        int Id PK
        int ProductId FK
        int UserId FK
        int Rating
    }

    WISHLIST_ITEMS {
        int Id PK
        int UserId FK
        int ProductId FK
    }

    CART_ITEMS {
        int Id PK
        int UserId FK
        int ProductId FK
        int VariantId FK
        int Quantity
    }

    ORDERS {
        int Id PK
        string OrderNumber UK
        int UserId FK
        decimal TotalAmount
        string PaymentStatus
        string Status
    }

    ORDER_ITEMS {
        int Id PK
        int OrderId FK
        int ProductId
        decimal PriceAtPurchase
        int Quantity
    }

    TRACKING_EVENTS {
        int Id PK
        int OrderId FK
        string Status
        datetime Timestamp
    }

    COUPONS {
        int Id PK
        string Code UK
        string Type
        decimal Value
    }

    NOTIFICATIONS {
        int Id PK
        int UserId FK
        string Type
        bool IsRead
    }

    CHAT_MESSAGES {
        int Id PK
        int UserId FK
        string Sender
    }

    USERS ||--o{ ADDRESSES : has
    USERS ||--o{ ORDERS : places
    USERS ||--o{ REVIEWS : writes
    USERS ||--o{ NOTIFICATIONS : receives
    USERS ||--o{ CHAT_MESSAGES : sends

    CATEGORIES ||--o{ PRODUCTS : contains
    CATEGORIES ||--o{ CATEGORIES : parent_child

    PRODUCTS ||--o{ PRODUCT_IMAGES : has
    PRODUCTS ||--o{ PRODUCT_VARIANTS : contains
    PRODUCTS ||--o{ REVIEWS : receives

    USERS ||--o{ WISHLIST_ITEMS : saves
    PRODUCTS ||--o{ WISHLIST_ITEMS : saved

    USERS ||--o{ CART_ITEMS : owns
    PRODUCTS ||--o{ CART_ITEMS : contains
    PRODUCT_VARIANTS ||--o{ CART_ITEMS : selected

    ORDERS ||--o{ ORDER_ITEMS : contains
    ORDERS ||--o{ TRACKING_EVENTS : tracks


# Relationship Matrix

| Parent Table            | Child Table                  | Relationship      |
| ----------------------- | ---------------------------- | ----------------- |
| Users                   | Addresses                    | One-to-Many       |
| Users                   | Orders                       | One-to-Many       |
| Users                   | Reviews                      | One-to-Many       |
| Users                   | Notifications                | One-to-Many       |
| Users                   | ChatMessages                 | One-to-Many       |
| Categories              | Products                     | One-to-Many       |
| Categories              | Categories                   | Self Relationship |
| Products                | ProductImages                | One-to-Many       |
| Products                | ProductVariants              | One-to-Many       |
| Products                | Reviews                      | One-to-Many       |
| Orders                  | OrderItems                   | One-to-Many       |
| Orders                  | TrackingEvents               | One-to-Many       |
| Users ↔ Products        | Many-to-Many (WishlistItems) |                   |
| Users ↔ ProductVariants | Many-to-Many (CartItems)     |                   |

# Table Summary

## Users

Stores customer and administrator accounts.

**Key Attributes**

* Phone (Unique)
* Name
* Email
* Role
## Addresses

Stores multiple shipping addresses for a user.

**Relationship**

* One User → Many Addresses
## OtpRecords

Stores OTP verification records for login and authentication.
## Categories

Supports hierarchical categories using ParentId.

Example:
Fashion
└── Shoes
    ├── Running
    ├── Sneakers
    └── Boots

## Products

Stores product catalog information.

**Contains**

* Product Details
* Brand Information
* Pricing Information
## ProductImages

Stores multiple images per product.

**Relationship**

* One Product → Many Images

## ProductVariants

Stores SKU-based inventory.

Example:
Nike Air Max

SKU001 → Black → Size 8
SKU002 → Black → Size 9
SKU003 → White → Size 8


## WishlistItems

Stores products saved by users.

**Relationship**

* User ↔ Product
## CartItems

Stores products added to cart.

**Relationship**

* User ↔ ProductVariant
## Reviews

Stores product ratings and feedback.

**Relationship**

* User → Review
* Product → Review

## Orders

Stores order information.

**Contains**

* Shipping Snapshot
* Payment Snapshot
* Coupon Snapshot
* Order Status

## OrderItems

Stores purchased product snapshots.

Purpose:

Even if product data changes later, order history remains unchanged.

Stores:

* Product Name
* SKU
* Color
* Size
* PriceAtPurchase

## TrackingEvents

Stores shipment tracking history.

Example Flow:
Placed
 ↓
Confirmed
 ↓
Packed
 ↓
Shipped
 ↓
Out For Delivery
 ↓
Delivered

## Coupons

Stores promotional discounts.

Supports:

* Percentage Discount
* Flat Discount
* Usage Limits
* Expiry Dates

## Notifications

Stores order, system, and promotional notifications.

## ChatMessages

Stores AI Assistant and Customer Support conversations.
# Business Flow
User
 │
 ▼
Cart
 │
 ▼
Checkout
 │
 ▼
Coupon Applied
 │
 ▼
Order Created
 │
 ▼
Payment
 │
 ▼
Tracking
 │
 ▼
Delivered


User
 │
 ├── Wishlist
 ├── Reviews
 ├── Notifications
 └── Chat Support


Category
 │
 ▼
Product
 │
 ├── Images
 ├── Variants
 └── Reviews
