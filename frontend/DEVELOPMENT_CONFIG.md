# Development Configuration Guide

## How Configuration Works in Development (`npm run dev`)

### ✅ Yes, `config.js` is Used!

When you run `npm run dev`, the application **does use** the parameters from `public/config.js`. Here's how it works:

### Configuration Loading Flow

1. **Vite Dev Server** serves files from the `public` folder at the root URL
2. **index.html** loads `config.js` via: `<script src="/config.js"></script>`
3. **config.js** sets `window.__CLINICCARE_CONFIG__` with your configuration
4. **Runtime config loader** (`src/core/config/runtime.ts`) reads from `window.__CLINICCARE_CONFIG__`
5. **Application code** uses the runtime config values

### Configuration Priority in Development

The application checks configuration in this order:

1. **`config.js` (Runtime)** - ✅ **Highest Priority**
   - Loaded from `public/config.js`
   - Available at `window.__CLINICCARE_CONFIG__`
   - Can be modified without restarting dev server (just refresh browser)

2. **Environment Variables** (`.env` files)
   - `VITE_API_BASE_URL`
   - `VITE_API_URL`
   - `VITE_SIGNALR_URL`
   - etc.

3. **Default Values** - Hardcoded fallbacks

### Current Development Setup

**File: `public/config.js`**
```javascript
window.__CLINICCARE_CONFIG__ = {
  API_BASE_URL: 'http://localhost:7000',
  API_URL: 'http://localhost:7000/api',
  SIGNALR_URL: 'http://localhost:7000',
  ENABLE_NOTIFICATIONS: true,
  ENABLE_SIGNALR: true,
  DEV_SUBDOMAIN: 'demo',
  ENVIRONMENT: 'development'
};
```

**Vite Proxy Configuration** (`vite.config.ts`)
```typescript
proxy: {
  '/api': {
    target: 'http://localhost:7000',
    changeOrigin: true,
    secure: false
  }
}
```

### Important Notes

#### 1. API URL Configuration

The `API_URL` in `config.js` should be set to the **full URL** (e.g., `http://localhost:7000/api`), but the Vite proxy will still work because:

- If you use relative URLs (like `/api/...`), Vite proxy handles them
- If you use full URLs (like `http://localhost:7000/api/...`), the browser makes direct requests (bypassing proxy)

**Recommendation for Development:**
- Set `API_URL: 'http://localhost:7000/api'` in `config.js` (full URL)
- The application will use this directly
- This matches production behavior

#### 2. Hot Module Replacement (HMR)

- Changes to `config.js` require a **browser refresh** to take effect
- HMR doesn't reload the config.js script automatically
- After editing `config.js`, refresh the browser (F5 or Ctrl+R)

#### 3. Testing Configuration Changes

1. Edit `public/config.js`
2. Save the file
3. Refresh browser (F5)
4. Check browser console: `window.__CLINICCARE_CONFIG__` should show your values
5. Verify API calls use the new URL

### Example: Changing API URL in Development

**Step 1:** Edit `public/config.js`
```javascript
window.__CLINICCARE_CONFIG__ = {
  API_BASE_URL: 'http://localhost:8000',  // Changed port
  API_URL: 'http://localhost:8000/api',
  // ... rest of config
};
```

**Step 2:** Refresh browser

**Step 3:** Verify in browser console:
```javascript
// Open browser console and type:
window.__CLINICCARE_CONFIG__
// Should show your updated values
```

**Step 4:** Check network tab - API calls should go to `http://localhost:8000/api`

### Development vs Production

| Aspect | Development (`npm run dev`) | Production (IIS) |
|--------|----------------------------|-------------------|
| Config File | `public/config.js` | `dist/config.js` |
| Served By | Vite dev server | IIS |
| Hot Reload | Yes (but config.js needs refresh) | No |
| Can Edit | Yes, edit `public/config.js` | Yes, edit `dist/config.js` |
| Takes Effect | After browser refresh | After browser refresh |

### Troubleshooting

#### Config.js Not Loading
- Check browser console for 404 error on `/config.js`
- Verify `public/config.js` exists
- Check Vite dev server is running
- Try accessing `http://localhost:3000/config.js` directly

#### Configuration Not Applied
- Refresh browser after editing `config.js`
- Check browser console: `window.__CLINICCARE_CONFIG__`
- Verify no JavaScript errors in console
- Check that runtime config loader is working

#### API Calls Using Wrong URL
- Check `API_URL` value in `config.js`
- Verify `apiClient.ts` is using `API_URL` from config
- Check browser Network tab to see actual requests
- Verify Vite proxy settings if using relative URLs

### Quick Reference

**To change API URL in development:**
1. Edit `frontend/public/config.js`
2. Change `API_BASE_URL` and `API_URL` values
3. Save file
4. Refresh browser (F5)
5. Done! ✅

**To verify configuration:**
```javascript
// In browser console:
console.log(window.__CLINICCARE_CONFIG__);
console.log(window.__CLINICCARE_CONFIG__?.API_URL);
```

### Summary

✅ **Yes, `npm run dev` uses `config.js` parameters**
- Config is loaded from `public/config.js`
- Takes effect after browser refresh
- Can be modified without restarting dev server
- Highest priority over environment variables

