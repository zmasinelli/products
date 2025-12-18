# Products Frontend

Angular frontend application for the Products API.

## Prerequisites

- Node.js (v20.19+ or v22.12+)
- npm or yarn

## Installation

```bash
npm install
```

## Development

Run the development server:

```bash
npm start
```

The app will be available at `http://localhost:4200`.

## Build

Build for production:

```bash
npm run build
```

## Configuration

API URL is configured in `src/environments/environment.ts` (development) and `src/environments/environment.prod.ts` (production).

Default development API URL: `http://localhost:5292`

## Features

- Product list view with Name, Price, Category Name, and Stock Quantity
- Error handling with user-friendly messages
- Loading states
- Responsive design
- Accessibility support

