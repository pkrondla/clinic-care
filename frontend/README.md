# ClinicCare Frontend

Modern React 19 frontend for the ClinicCare homoeopathy clinic management system.

## 🚀 Tech Stack

- **React 19** - Latest React with concurrent features
- **TypeScript** - Strong typing throughout
- **Vite** - Ultra-fast build tool and dev server
- **Ant Design** - Enterprise-class UI components
- **TanStack Query v5** - Powerful server state management
- **Zustand** - Lightweight client state management
- **React Router DOM** - Client-side routing
- **SignalR** - Real-time communication
- **Axios** - HTTP client
- **React Hot Toast** - Beautiful notifications

## 📁 Project Structure

```
src/
├── components/           # Reusable UI components
│   ├── layout/          # Layout components
│   └── ui/              # Basic UI components
├── hooks/               # Custom React hooks
│   ├── queries/         # TanStack Query hooks
│   └── useSignalR.ts    # Real-time hooks
├── pages/               # Page components
│   ├── auth/            # Authentication pages
│   ├── dashboard/       # Dashboard pages
│   ├── appointments/    # Appointment management
│   └── ...              # Other feature pages
├── providers/           # Context providers
├── services/            # API services
├── stores/              # Zustand stores
├── types/               # TypeScript definitions
└── utils/               # Helper functions
```

## 🛠️ Development Setup

### Prerequisites
- Node.js 18+ 
- npm or yarn

### Installation

1. **Install dependencies**
   ```bash
   npm install
   ```

2. **Environment setup**
   Create a `.env` file with:
   ```env
   VITE_API_BASE_URL=http://localhost:7000/api
   VITE_SIGNALR_URL=http://localhost:7000
   VITE_DEV_SUBDOMAIN=healthcareplus
   ```

3. **Start development server**
   ```bash
   npm run dev
   ```

4. **Access the application**
   ```
   http://localhost:3000
   ```

## 🔑 Authentication

### Demo Credentials

**Organization:** healthcareplus (subdomain)

- **Admin:** admin@healthcareplus.com
- **Doctor:** dr.smith@healthcareplus.com
- **Staff:** reception1@healthcareplus.com
- **Patient:** patient1@email.com

*Note: All demo accounts use the same placeholder password that needs to be set in the backend.*

## 🏗️ Architecture Highlights

### Multi-Tenancy
- Subdomain-based tenant resolution
- Automatic tenant context in API calls
- Cross-organization user access support

### Real-time Features
- SignalR integration for live updates
- Real-time queue management
- Instant appointment status changes
- Live notifications

### State Management
- **Server State:** TanStack Query for API data
- **Client State:** Zustand for UI state
- **Auth State:** Persistent auth store
- **UI State:** Theme, modals, notifications

### Performance
- **Code splitting** by routes
- **Lazy loading** of components
- **Optimistic updates** for better UX
- **Smart caching** with TanStack Query

## 🎨 UI Components

### Role-Based Interfaces

**Dashboard Views:**
- **Super Admin:** System overview, global management
- **Admin:** Organization management, reports
- **Doctor:** Patient queue, consultations, prescriptions
- **Staff:** Patient registration, appointments, inventory
- **Patient:** Personal records, appointments, prescriptions

### Key Features
- **Responsive design** for all screen sizes
- **Dark/light theme** support
- **PWA capabilities** for mobile usage
- **Offline support** for critical features
- **Print-friendly** pages

## 📱 Mobile Support

- Responsive design for tablets and phones
- Touch-friendly interfaces
- PWA installation support
- Offline functionality for essential features

## 🔧 API Integration

### HTTP Client
- Axios with interceptors
- Automatic token management
- Error handling and notifications
- Request/response logging

### Real-time Communication
- SignalR for live updates
- Automatic reconnection
- Connection state management
- Event-driven updates

## 🚀 Build & Deployment

### Development
```bash
npm run dev        # Start dev server
npm run build      # Build for production
npm run preview    # Preview production build
npm run lint       # Run ESLint
```

### Production Build
- Optimized bundle with tree-shaking
- PWA manifest and service worker
- Gzip compression
- Asset optimization

### Deployment Options
- **Static hosting:** Vercel, Netlify, GitHub Pages
- **Docker:** Container-ready build
- **CDN:** Optimized for edge delivery

## 🧪 Testing

```bash
npm run test       # Run unit tests
npm run test:watch # Watch mode
npm run test:ui    # Test UI (Vitest UI)
```

## 📈 Performance Metrics

- **First Contentful Paint:** < 1.5s
- **Largest Contentful Paint:** < 2.5s
- **Cumulative Layout Shift:** < 0.1
- **Bundle size:** ~1.2MB (gzipped: ~393KB)

## 🔮 Future Enhancements

- [ ] Advanced reporting dashboard
- [ ] Video consultation integration
- [ ] Mobile app (React Native)
- [ ] Multi-language support
- [ ] Advanced search and filtering
- [ ] Bulk operations
- [ ] Export functionality
- [ ] Advanced analytics

## 📄 License

MIT License - see the LICENSE file for details.

---

**ClinicCare Frontend** - Modern, efficient, and user-friendly interface for homoeopathy clinic management 🌿