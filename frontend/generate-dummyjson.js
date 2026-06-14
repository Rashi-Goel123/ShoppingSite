const https = require('https');
const fs = require('fs');
const path = require('path');

const url = 'https://dummyjson.com/products?limit=150';

https.get(url, (res) => {
  let data = '';

  res.on('data', (chunk) => {
    data += chunk;
  });

  res.on('end', () => {
    try {
      const json = JSON.parse(data);

      const categoryMap = {
        'mens-shirts': { name: 'Shirts', gender: 'men' },
        'mens-shoes': { name: 'Sneakers', gender: 'men' },
        'mens-watches': { name: 'Watches', gender: 'men' },
        'womens-dresses': { name: 'Dresses', gender: 'women' },
        'womens-shoes': { name: 'Sandals & Flats', gender: 'women' },
        'womens-watches': { name: 'Watches', gender: 'women' },
        'womens-bags': { name: 'Bags & Wallets', gender: 'women' },
        'womens-jewellery': { name: 'Jewellery', gender: 'women' },
        'sunglasses': { name: 'Sunglasses', gender: 'unisex' },
        'tops': { name: 'Tops & Tees', gender: 'women' }
      };

      const validCategories = Object.keys(categoryMap);

      const fashionProducts = json.products.filter((p) => {
        const category =
          typeof p.category === 'string'
            ? p.category
            : p.category?.slug;

        return validCategories.includes(category);
      });

      let products = [];
      let idCounter = 1;

      const shuffled = [...fashionProducts].sort(
        () => 0.5 - Math.random()
      );

      const selectedProducts = shuffled.slice(0, 50);

      for (const p of selectedProducts) {
        const category =
          typeof p.category === 'string'
            ? p.category
            : p.category?.slug;

        const mapping = categoryMap[category];

        if (!mapping) continue;

        const discountPercent =
          Math.floor(Math.random() * 40) + 10;

        const basePrice = Math.floor(p.price * 80);

        const discountedPrice = Math.floor(
          basePrice * (1 - discountPercent / 100)
        );

        products.push({
          id: idCounter,
          title: p.title,
          slug:
            'product-' +
            idCounter +
            '-' +
            p.title
              .toLowerCase()
              .replace(/[^a-z0-9]+/g, '-'),

          brand: p.brand || 'Style Hub',
          gender: mapping.gender,
          basePrice,
          firstImage: p.thumbnail,

          ratingAverage: parseFloat(
            (Math.random() * 1.5 + 3.5).toFixed(1)
          ),

          ratingCount:
            Math.floor(Math.random() * 300) + 10,

          isFeatured: Math.random() > 0.7,

          categoryName: mapping.name,

          discountedPrice,
          mrp: basePrice,
          discountPercent,

          description: p.description,

          images:
            p.images && p.images.length
              ? p.images
              : [p.thumbnail]
        });

        idCounter++;
      }

      const listItems = products.map((p) => {
        const { description, images, ...rest } = p;
        return rest;
      });

      const fileContent = `
import { ProductListItem, ProductDetail } from '../core/models/models';

export const MOCK_PRODUCTS: ProductListItem[] = ${JSON.stringify(
        listItems,
        null,
        2
      ).replace(/"([^"]+)":/g, '$1:')};

export const MOCK_PRODUCTS_DETAIL: ProductDetail[] = ${JSON.stringify(
        products,
        null,
        2
      )}.map((p: any) => ({
  id: p.id,
  title: p.title,
  slug: p.slug,

  description:
    p.description ||
    ('Premium quality ' +
      p.title.toLowerCase() +
      ' from ' +
      p.brand +
      '. Crafted with attention to detail and designed for modern fashion enthusiasts.'),

  brand: p.brand,
  gender: p.gender,
  material: 'Premium Fabric',

  basePrice: p.basePrice,

  ratingAverage: p.ratingAverage,
  ratingCount: p.ratingCount,

  isFeatured: p.isFeatured,

  category: {
    id: 1,
    name: p.categoryName,
    slug: p.categoryName.toLowerCase().replace(/ /g, '-'),
    parentId: undefined
  },

  images: p.images.map((url: string, i: number) => ({
    id: i + 1,
    url,
    displayOrder: i
  })),

  variants: [
    {
      id: p.id * 10 + 1,
      sku: 'SKU-' + p.id + '-S',
      color: 'Black',
      colorHex: '#000000',
      size: 'S',
      price: p.discountedPrice || p.basePrice,
      mrp: p.mrp || p.basePrice,
      stock: 25
    },
    {
      id: p.id * 10 + 2,
      sku: 'SKU-' + p.id + '-M',
      color: 'Black',
      colorHex: '#000000',
      size: 'M',
      price: p.discountedPrice || p.basePrice,
      mrp: p.mrp || p.basePrice,
      stock: 40
    },
    {
      id: p.id * 10 + 3,
      sku: 'SKU-' + p.id + '-L',
      color: 'Black',
      colorHex: '#000000',
      size: 'L',
      price: p.discountedPrice || p.basePrice,
      mrp: p.mrp || p.basePrice,
      stock: 30
    }
  ],

  reviews: [
    {
      id: 1,
      rating: 5,
      title: 'Excellent quality!',
      comment:
        'Absolutely love this product. The material feels premium and the fit is perfect.',
      userName: 'Priya S.',
      isVerifiedPurchase: true,
      createdAt: '2026-06-10T10:00:00Z'
    }
  ]
}));
`;

      const outputDir = path.join(
        'src',
        'app',
        'mock-data'
      );

      fs.mkdirSync(outputDir, { recursive: true });

      fs.writeFileSync(
        path.join(outputDir, 'mock-products.ts'),
        fileContent,
        'utf8'
      );

      console.log(
        '✅ Successfully generated mock-products.ts'
      );
      console.log(
        `✅ Products generated: ${products.length}`
      );
    } catch (err) {
      console.error('❌ Error:', err);
    }
  });
}).on('error', (err) => {
  console.error('❌ Request Error:', err);
});