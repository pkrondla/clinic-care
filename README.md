# ClinicCare - Homoeopathy Clinic Management System

A comprehensive multi-tenant clinic management system built with .NET 9, React, and SQL Server.

## 🏥 Features

### Multi-Tenancy Support
- Subdomain-based tenant resolution (`clinic1.yourapp.com`)
- Complete data isolation between organizations
- Cross-organization user access for doctors and patients

### Core Functionality
- **Patient Management**: Registration, medical history, cross-clinic access
- **Appointment System**: Token-based queue management with daily reset
- **Doctor Management**: Multi-clinic scheduling, availability tracking
- **Consultation Records**: Complete medical encounter documentation
- **Prescription System**: Dual templates (internal/patient), medicine tracking
- **Inventory Management**: Per-clinic stock with transfer capabilities
- **Billing & Invoicing**: Comprehensive billing with multiple payment types
- **Communication**: WhatsApp/Email/SMS notifications

### User Roles
- **Super Admin**: Global system management
- **Admin**: Organization management
- **Doctor**: Consultations, prescriptions, multi-clinic access
- **Staff**: Reception and pharmacy operations
- **Patient**: View records, online consultations, appointments

## 🏗️ Architecture

### Backend (.NET 9 Web API)
- **Clean Architecture** with Domain, Application, Infrastructure layers
- **Multi-tenant data isolation** with row-level security
- **JWT Authentication** with role-based authorization
- **Entity Framework Core** with SQL Server
- **MediatR** for CQRS pattern
- **SignalR** for real-time queue updates
- **Serilog** for structured logging

### Database Design
- **Single Database, Multiple Tenants** approach
- **Global Medicine Database** shared across all tenants
- **Per-clinic inventory** with audit trails
- **Optimized indexes** for tenant-scoped queries

### Frontend (React - To be implemented)
- Role-based dashboards
- Real-time queue management
- Responsive design for mobile/tablet use
- PWA support for offline capabilities

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB for development)
- Node.js 18+ (for React frontend)
- Visual Studio 2022 or VS Code

### Database Setup

1. **Create Database and Tables**
   ```bash
   # Run the SQL scripts in order
   sqlcmd -S (localdb)\mssqllocaldb -i database/001_CreateTables.sql
   sqlcmd -S (localdb)\mssqllocaldb -i database/002_CreateIndexes.sql
   sqlcmd -S (localdb)\mssqllocaldb -i database/003_SeedData.sql
   ```

2. **Update Connection String**
   ```json
   // appsettings.Development.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ClinicCareDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

### Running the Backend API

1. **Navigate to API project**
   ```bash
   cd src/ClinicCare.API
   ```

2. **Run the application**
   ```bash
   dotnet run
   ```

3. **Access Swagger UI**
   ```
   https://localhost:7000/swagger
   ```

### Sample Data

The system includes sample data for testing:

- **Organization**: HealthCare Plus Clinics (subdomain: `healthcareplus`)
- **Admin**: admin@healthcareplus.com
- **Doctors**: dr.smith@healthcareplus.com, dr.johnson@healthcareplus.com
- **Staff**: reception1@healthcareplus.com, pharmacy1@healthcareplus.com
- **Patients**: patient1@email.com through patient5@email.com

### Testing Multi-Tenancy

Add subdomain header for testing:
```bash
curl -H "X-Tenant-Subdomain: healthcareplus" https://localhost:7000/api/appointments
```

## 📋 API Endpoints

### Authentication
- `POST /api/auth/login` - User login with clinic selection
- `POST /api/auth/logout` - User logout
- `GET /api/auth/me` - Current user info

### Appointments
- `GET /api/appointments` - List appointments (role-filtered)
- `GET /api/appointments/{id}` - Get appointment details
- `GET /api/appointments/queue/{doctorId}/{clinicId}` - Real-time queue

### Real-time Features
- SignalR Hub: `/queueHub` - Real-time queue updates

## 🔧 Configuration

### JWT Settings
```json
{
  "Jwt": {
    "Key": "your-super-secret-key-here-at-least-32-characters-long",
    "Issuer": "ClinicCare",
    "Audience": "ClinicCareUsers",
    "ExpirationInDays": 7
  }
}
```

### Feature Flags
```json
{
  "Features": {
    "EnableWhatsAppIntegration": true,
    "EnableEmailNotifications": true,
    "EnableSMSNotifications": false
  }
}
```

## 🚀 Deployment

### Docker Support (Coming Soon)
```dockerfile
# Dockerfile for containerized deployment
```

### Cloud Deployment
- **Azure**: App Service + SQL Database
- **AWS**: Elastic Beanstalk + RDS
- **On-Premises**: IIS + SQL Server

## 🔄 Development Status

### ✅ Completed
- [x] Database schema design
- [x] Multi-tenant architecture
- [x] Authentication & authorization
- [x] Core domain entities
- [x] Basic API endpoints
- [x] Real-time SignalR setup

### 🚧 In Progress
- [ ] Complete API implementation
- [ ] React frontend development
- [ ] WhatsApp integration
- [ ] Payment gateway integration
- [ ] Reporting system

### 📋 Upcoming
- [ ] Mobile app (React Native)
- [ ] Advanced analytics
- [ ] Backup and restore
- [ ] Multi-language support

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 📞 Support

For support and questions:
- Create an issue in the repository
- Email: support@cliniccare.com
- Documentation: [Wiki](link-to-wiki)

---

**ClinicCare** - Empowering homoeopathy clinics with modern technology 🌿
