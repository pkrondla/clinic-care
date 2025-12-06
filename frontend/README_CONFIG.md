# Configuration Guide for ClinicCare Frontend

## Overview

The ClinicCare frontend supports runtime configuration through a `config.js` file, similar to .NET's `appsettings.json`. This allows you to change API URLs and other settings without rebuilding the application.

## Configuration Files

### 1. `appsettings.json` / `appsettings.Production.json`
- **Purpose**: Reference files that document configuration options
- **Location**: Root of `frontend` folder
- **Usage**: Edit these files, then generate `config.js` using the script

### 2. `public/config.js`
- **Purpose**: Runtime configuration file loaded by the browser
- **Location**: `frontend/public/config.js` (copied to `dist` during build)
- **Usage**: This is the actual file used by the application at runtime

### 3. `public/config.template.js`
- **Purpose**: Template for creating `config.js`
- **Usage**: Reference when manually creating `config.js`

## Configuration Options

```javascript
window.__CLINICCARE_CONFIG__ = {
  // Backend API base URL (without /api suffix)
  API_BASE_URL: 'http://localhost:7000',
  
  // Full API URL (with /api suffix)
  API_URL: 'http://localhost:7000/api',
  
  // SignalR WebSocket URL
  SIGNALR_URL: 'http://localhost:7000',
  
  // Feature flags
  ENABLE_NOTIFICATIONS: true,
  ENABLE_SIGNALR: true,
  
  // Development subdomain (for local testing)
  DEV_SUBDOMAIN: 'demo',
  
  // Environment: 'development', 'staging', 'production'
  ENVIRONMENT: 'development'
};
```

## Generating config.js

### From appsettings.json

```bash
# Generate from appsettings.json (development)
npm run generate-config

# Generate from appsettings.Production.json (production)
npm run generate-config:prod
```

### Manual Creation

1. Copy `public/config.template.js` to `public/config.js`
2. Update the values in `config.js` with your production settings
3. The file will be automatically copied to `dist` during build

## Build Process

### Development Build
```bash
npm run build
```
- Uses existing `config.js` in `public` folder
- Copies `config.js` to `dist` folder

### Production Build
```bash
npm run build:prod
```
- Generates `config.js` from `appsettings.Production.json`
- Builds the application
- Ready for deployment

## Configuration Priority

The application loads configuration in this order (highest to lowest priority):

1. **Runtime config** (`config.js`) - Can be modified after deployment
2. **Environment variables** (`.env` files) - Used during development
3. **Default values** - Hardcoded fallbacks

## IIS Deployment

### Step 1: Build for Production
```bash
npm run build:prod
```

### Step 2: Deploy to IIS
1. Copy contents of `dist` folder to IIS website directory
2. Ensure `config.js` is in the root of the deployed folder
3. Edit `config.js` in IIS directory if needed (no rebuild required)

### Step 3: Update Configuration After Deployment
1. Navigate to IIS website directory
2. Edit `config.js` file directly
3. Changes take effect on next page load (no restart needed)

## Example: Updating API URL

### Before Deployment
1. Edit `appsettings.Production.json`:
```json
{
  "API_BASE_URL": "https://api.yourdomain.com",
  "API_URL": "https://api.yourdomain.com/api",
  "SIGNALR_URL": "https://api.yourdomain.com"
}
```

2. Generate config:
```bash
npm run generate-config:prod
```

3. Build:
```bash
npm run build:prod
```

### After Deployment (Runtime Update)
1. Navigate to IIS directory: `C:\inetpub\wwwroot\cliniccare`
2. Edit `config.js`:
```javascript
window.__CLINICCARE_CONFIG__ = {
  API_BASE_URL: 'https://api.yourdomain.com',
  API_URL: 'https://api.yourdomain.com/api',
  // ... rest of config
};
```

3. Save - changes are live immediately

## Environment-Specific Configuration

### Development
- File: `appsettings.json`
- API URL: `http://localhost:7000`
- Environment: `development`

### Production
- File: `appsettings.Production.json`
- API URL: Your production API URL
- Environment: `production`

## Troubleshooting

### Config.js Not Loading
- Ensure `config.js` is in the root of `dist` folder
- Check browser console for 404 errors
- Verify script tag in `index.html`: `<script src="/config.js"></script>`

### API Calls Failing
- Verify `API_BASE_URL` in `config.js` is correct
- Check CORS configuration on backend
- Ensure backend is running and accessible

### Changes Not Taking Effect
- Clear browser cache
- Hard refresh (Ctrl+F5)
- Check browser console for errors

## Files Reference

| File | Purpose | Location |
|------|---------|----------|
| `appsettings.json` | Development config reference | `frontend/` |
| `appsettings.Production.json` | Production config reference | `frontend/` |
| `public/config.js` | Runtime config (used by app) | `frontend/public/` |
| `public/config.template.js` | Template for manual creation | `frontend/public/` |
| `scripts/generate-config.js` | Script to generate config.js | `frontend/scripts/` |

## Notes

- `appsettings.json` files are for reference/documentation
- React doesn't natively use `appsettings.json` like .NET
- Actual configuration is loaded from `config.js` at runtime
- `config.js` can be modified after deployment without rebuilding

