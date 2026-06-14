export const MOCK_CATEGORIES = [
  {
    id: 1, name: "Men's Wear", slug: 'mens-wear', icon: '👔', parentId: null,
    children: [
      { id: 7, name: 'T-Shirts & Polos', slug: 't-shirts-polos', icon: null, parentId: 1 },
      { id: 8, name: 'Shirts', slug: 'shirts', icon: null, parentId: 1 },
      { id: 9, name: 'Jeans & Trousers', slug: 'jeans-trousers', icon: null, parentId: 1 },
      { id: 10, name: 'Jackets & Blazers', slug: 'jackets-blazers', icon: null, parentId: 1 },
      { id: 11, name: 'Ethnic Wear', slug: 'ethnic-wear-men', icon: null, parentId: 1 },
      { id: 12, name: 'Activewear', slug: 'activewear-men', icon: null, parentId: 1 },
    ]
  },
  {
    id: 2, name: "Women's Wear", slug: 'womens-wear', icon: '👗', parentId: null,
    children: [
      { id: 13, name: 'Dresses', slug: 'dresses', icon: null, parentId: 2 },
      { id: 14, name: 'Tops & Tees', slug: 'tops-tees', icon: null, parentId: 2 },
      { id: 15, name: 'Sarees & Lehengas', slug: 'sarees-lehengas', icon: null, parentId: 2 },
      { id: 16, name: 'Jeans & Palazzos', slug: 'jeans-palazzos', icon: null, parentId: 2 },
      { id: 17, name: 'Jackets & Coats', slug: 'jackets-coats', icon: null, parentId: 2 },
    ]
  },
  {
    id: 3, name: 'Footwear', slug: 'footwear', icon: '👟', parentId: null,
    children: [
      { id: 18, name: 'Sneakers', slug: 'sneakers', icon: null, parentId: 3 },
      { id: 19, name: 'Formal Shoes', slug: 'formal-shoes', icon: null, parentId: 3 },
      { id: 20, name: 'Sandals & Flats', slug: 'sandals-flats', icon: null, parentId: 3 },
      { id: 21, name: 'Sports Shoes', slug: 'sports-shoes', icon: null, parentId: 3 },
      { id: 22, name: 'Boots', slug: 'boots', icon: null, parentId: 3 },
    ]
  },
  {
    id: 4, name: 'Accessories', slug: 'accessories', icon: '💎', parentId: null,
    children: [
      { id: 23, name: 'Watches', slug: 'watches', icon: null, parentId: 4 },
      { id: 24, name: 'Bags & Wallets', slug: 'bags-wallets', icon: null, parentId: 4 },
      { id: 25, name: 'Sunglasses', slug: 'sunglasses', icon: null, parentId: 4 },
      { id: 26, name: 'Jewellery', slug: 'jewellery', icon: null, parentId: 4 },
      { id: 27, name: 'Belts', slug: 'belts', icon: null, parentId: 4 },
    ]
  },
  {
    id: 5, name: "Kids' Wear", slug: 'kids-wear', icon: '🧒', parentId: null,
    children: [
      { id: 28, name: "Boys' Clothing", slug: 'boys-clothing', icon: null, parentId: 5 },
      { id: 29, name: "Girls' Clothing", slug: 'girls-clothing', icon: null, parentId: 5 },
    ]
  },
  {
    id: 6, name: 'Beauty & Grooming', slug: 'beauty-and-grooming', icon: '💄', parentId: null,
    children: [
      { id: 30, name: 'Skincare', slug: 'skincare', icon: null, parentId: 6 },
      { id: 31, name: 'Fragrances', slug: 'fragrances', icon: null, parentId: 6 },
      { id: 32, name: 'Haircare', slug: 'haircare', icon: null, parentId: 6 },
    ]
  },
];
