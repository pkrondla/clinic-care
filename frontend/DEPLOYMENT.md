# ClinicCare Frontend - IIS Deployment Guide

## Overview

This guide explains how to deploy the ClinicCare React frontend to IIS with runtime configuration support.

## Prerequisites

- IIS 7.5 or later with URL Rewrite module installed
- Node.js installed (for building)
- .NET Core hosting bundle (if deploying backend to same server)

## Build Steps

1. **Install dependencies**
   ```bash
   npm install
   ```

2. **Build for production**
   ```bash
   npm run build
   ```

3. **The build output will be in the `dist` folder**

## IIS Deployment

### 1. Copy Files to IIS

Copy the contents of the `dist` folder to your IIS website directory (e.g., `C:\inetpub\wwwroot\cliniccare`).

### 2. Configure Runtime Settings

The frontend uses a runtime configuration file (`config.js`) that can be modified without rebuilding:

1. Copy `public/config.template.js` to `public/config.js` (or create it in the dist folder)
2. Update `config.js` with your production API URL:

```javascript
window.__CLINICCARE_CONFIG__ = {
  API_BASE_URL: 'https://api.yourdomain.com',
  API_URL: 'https://api.yourdomain.com/api',
  SIGNALR_URL: 'https://api.yourdomain.com',
  ENABLE_NOTIFICATIONS: true,
  ENABLE_SIGNALR: true,
  DEV_SUBDOMAIN: 'demo',
  ENVIRONMENT: 'production'
};
```

**Important:** After building, copy `config.js` to the `dist` folder so it's included in the deployment.

### 3. IIS Configuration

1. **Install URL Rewrite Module**
   - Download from: https://www.iis.net/downloads/microsoft/url-rewrite
   - Install on the IIS server

2. **Copy web.config**
   - The `web.config` file is already included in the `public` folder
   - It will be copied to `dist` during build
   - This file handles:
     - React Router client-side routing
     - Static file serving
     - Security headers
     - Compression

3. **Configure Application Pool**
   - Set .NET CLR Version to "No Managed Code" (since this is a static React app)
   - Set Managed Pipeline Mode to "Integrated"

4. **Set Permissions**
   - Ensure IIS_IUSRS has read access to the website directory

### 4. Update Configuration After Deployment

To change the API URL after deployment:

1. Navigate to the IIS website directory
2. Edit `config.js` file
3. No rebuild or restart needed - changes take effect on next page load

## Configuration Priority

The application loads configuration in this order:

1. **Runtime config** (`config.js`) - Highest priority, can be changed without rebuild
2. **Environment variables** (`.env` files) - Used during development
3. **Default values** - Fallback if nothing else is set

## Environment-Specific Configuration

### Development
- Uses `config.js` with `ENVIRONMENT: 'development'`
- API URL: `http://localhost:7000`

### Production
- Uses `config.js` with `ENVIRONMENT: 'production'`
- API URL: Your production API URL

## Troubleshooting

### 404 Errors on Route Refresh
- Ensure URL Rewrite module is installed
- Check that `web.config` is in the root of the website directory

### API Calls Failing
- Check `config.js` has the correct API URL
- Verify CORS is configured on the backend
- Check browser console for errors

### Config.js Not Loading
- Ensure `config.js` is in the root of the `dist` folder
- Check browser console for 404 errors
- Verify the script tag is in `index.html`

## Files Reference

- `public/config.js` - Runtime configuration (update this for production)
- `public/config.template.js` - Template for creating config.js
- `public/web.config` - IIS configuration
- `appsettings.json` - Reference file (for documentation, not used by React)
- `appsettings.Production.json` - Production reference

## Notes

- The `appsettings.json` files are provided for reference/documentation purposes
- React doesn't natively use `appsettings.json` like .NET
- The actual configuration is loaded from `config.js` at runtime
- You can use `appsettings.json` as a reference when creating/updating `config.js`

