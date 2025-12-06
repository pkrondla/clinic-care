#!/usr/bin/env node
/**
 * Generate config.js from appsettings.json
 * Usage: node scripts/generate-config.js [environment]
 * Example: node scripts/generate-config.js production
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

// Read appsettings.json
let appSettings = {};
try {
  const content = fs.readFileSync(appSettingsPath, 'utf8');
  appSettings = JSON.parse(content);
  console.log(`✓ Loaded ${appSettingsFile}`);
} catch (error) {
  console.error(`✗ Error reading ${appSettingsFile}:`, error.message);
  process.exit(1);
}

// Generate config.js content
const configContent = `// Runtime configuration for ClinicCare Frontend
// This file is auto-generated from ${appSettingsFile}
// Generated at: ${new Date().toISOString()}
// Environment: ${environment}

window.__CLINICCARE_CONFIG__ = {
  API_BASE_URL: '${appSettings.API_BASE_URL || 'http://localhost:7000'}',
  API_URL: '${appSettings.API_URL || appSettings.API_BASE_URL + '/api' || 'http://localhost:7000/api'}',
  SIGNALR_URL: '${appSettings.SIGNALR_URL || appSettings.API_BASE_URL || 'http://localhost:7000'}',
  ENABLE_NOTIFICATIONS: ${appSettings.ENABLE_NOTIFICATIONS !== false},
  ENABLE_SIGNALR: ${appSettings.ENABLE_SIGNALR !== false},
  DEV_SUBDOMAIN: '${appSettings.DEV_SUBDOMAIN || 'demo'}',
  ENVIRONMENT: '${appSettings.ENVIRONMENT || environment}'
};
`;

// Write config.js
try {
  fs.writeFileSync(configPath, configContent, 'utf8');
  console.log(`✓ Generated config.js from ${appSettingsFile}`);
  console.log(`  Location: ${configPath}`);
  console.log(`  Environment: ${environment}`);
} catch (error) {
  console.error(`✗ Error writing config.js:`, error.message);
  process.exit(1);
}

