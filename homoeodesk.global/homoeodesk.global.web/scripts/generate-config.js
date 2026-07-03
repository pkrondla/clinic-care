#!/usr/bin/env node
/**
 * Generate config.js from appsettings.json
 * Usage: node scripts/generate-config.js [environment]
 */

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const environment = process.argv[2] || 'development';
const appSettingsFile = environment === 'production'
  ? 'appsettings.Production.json'
  : 'appsettings.json';

const appSettingsPath = path.join(__dirname, '..', appSettingsFile);
const configPath = path.join(__dirname, '..', 'public', 'config.js');

let appSettings = {};
try {
  const content = fs.readFileSync(appSettingsPath, 'utf8');
  appSettings = JSON.parse(content);
  console.log(`✓ Loaded ${appSettingsFile}`);
} catch (error) {
  console.error(`✗ Error reading ${appSettingsFile}:`, error.message);
  process.exit(1);
}

const apiBase = appSettings.API_BASE_URL || 'http://localhost:7100';
const apiUrl = appSettings.API_URL || `${apiBase}/api`;

const configContent = `// Runtime configuration for HomoeoDesk Frontend
// Auto-generated from ${appSettingsFile}
// Generated at: ${new Date().toISOString()}
// Environment: ${environment}

window.__HOMOEODESK_CONFIG__ = {
  API_BASE_URL: '${apiBase}',
  API_URL: '${apiUrl}',
  SIGNALR_URL: '${appSettings.SIGNALR_URL || apiBase}',
  ENABLE_NOTIFICATIONS: ${appSettings.ENABLE_NOTIFICATIONS !== false},
  ENABLE_SIGNALR: ${appSettings.ENABLE_SIGNALR !== false},
  DEV_SUBDOMAIN: '${appSettings.DEV_SUBDOMAIN || 'demo'}',
  ENVIRONMENT: '${appSettings.ENVIRONMENT || environment}'
};
`;

try {
  fs.writeFileSync(configPath, configContent, 'utf8');
  console.log(`✓ Generated config.js from ${appSettingsFile}`);
  console.log(`  Location: ${configPath}`);
} catch (error) {
  console.error(`✗ Error writing config.js:`, error.message);
  process.exit(1);
}
