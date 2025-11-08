# ClinicCare - Homoeopathy Clinic Management System
## Requirements Specification for LLM Code Development

---

## 🎯 **PROJECT OVERVIEW**

**ClinicCare** is a comprehensive multi-tenant homoeopathy clinic management system designed to handle multiple organizations, each with multiple clinics, supporting doctors who work across different clinics. The system manages patient registration, appointments, medical records, prescriptions, inventory, billing, and communication.

**Business Model**: Multiple clinic groups (organizations) can use the same software instance, with each organization having its own subdomain and dedicated database for complete data isolation.

---

## 🏗️ **TECHNICAL ARCHITECTURE**

### **Technology Stack**
- **Backend**: .NET 9 Web API with Clean Architecture
- **Frontend**: React Latest with TypeScript
- **Database**: 
  - Global Database: SQL Server (Organizations, Subscriptions, Global Medicines)
  - Tenant Databases: Separate SQL Server database per tenant
- **Deployment**: On-premises or cloud deployment support
- **Authentication**: JWT-based authentication with role-based authorization

### **Architecture Pattern**
- **Clean Architecture**: 
  - Domain Layer: Separate Global and Tenant entities
  - Application Layer: Distinct Global and Tenant features
  - Infrastructure Layer: Separate database contexts and services
  - API Layer: Area-based separation for Global and Tenant endpoints
- **Multi-Tenancy**: 
  - Global System: Accessed via yourapp.com
  - Tenant System: Accessed via {tenant}.yourapp.com
  - Separate database per tenant for complete isolation
  - Dynamic database context management
- **Module Separation**:
  - Clear separation between Global and Tenant features
  - Shared core functionality
  - Independent routing and authentication flows
- **CQRS**: Command Query Responsibility Segregation with MediatR
- **Repository Pattern**: Separate repositories for Global and Tenant data
- **SignalR**: Real-time communication for queue updates
- **Frontend Structure**:
  - Shared component library
  - Separate entry points for Global and Tenant apps
  - Common authentication and state management

---

## 🔑 **CORE BUSINESS REQUIREMENTS**

### **1. Multi-Tenancy & Organization Structure**
- **Tenant Resolution**: Subdomain-based (`organization1.yourapp.com`)
- **Data Isolation**: Complete separation between organizations via `OrganizationId`
- **Organization Structure**: 
  - One organization can have multiple clinics
  - Each clinic can have multiple doctors
  - Single doctor can have their own clinic
  - All use the same software instance

### **2. User Management & Access Control**
- **User Roles**:
  - **Super Admin**: Global system management, medicine database maintenance
  - **Organization Admin**: Organization and clinic management
  - **Doctor**: Patient consultations, prescriptions, multi-clinic access
  - **Reception Staff**: Patient registration, appointment booking, token management
  - **Pharmacy Staff**: Medicine dispensing, inventory management
  - **Patient**: View own records, prescriptions, queue position

- **Account Management**:
  - **Doctors**: One account with multiple organization access (admin-controlled)
  - **Patients**: One unified account across all organizations
  - **Staff**: Organization-scoped accounts

### **3. Clinic & Doctor Management**
- **Clinic Selection**: After login, users select clinic (if multiple exist)
- **Doctor Availability**: 
  - No fixed schedules initially
  - Doctors update availability per clinic per day
  - Can work at multiple clinics on same day (morning/evening sessions)
- **Cross-Clinic Access**: Doctors can see patients from different clinics within organization

---

## 🏥 **PATIENT & APPOINTMENT MANAGEMENT**

### **4. Patient Registration & Management**
- **Unique Patient ID**: System-generated unique identifier per patient
- **Cross-Clinic Access**: Patients can visit any clinic within organization
- **Medical History**: Complete history accessible across all clinics in organization
- **Patient Portal**: Login access to view own records and prescriptions

### **5. Appointment & Token System**
- **Appointment Booking**: 
  - Reception staff books appointments
  - Phone/WhatsApp booking support
  - Only available doctors for selected date displayed
- **Token System**:
  - **Per Doctor Per Clinic**: Separate token queues for each doctor at each clinic
  - **Daily Reset**: Token numbers reset every day
  - **Queue Management**: Real-time queue position for patients
  - **Token Allocation**: Sequential numbering per doctor per clinic per day

### **6. Consultation Workflow**
- **In-Person Consultation**:
  - Patient visits clinic with token
  - Doctor examines patient in token order
  - Diagnosis and prescription creation
  - Medicine dispensing from clinic pharmacy
- **Teleconsultation**:
  - Phone/WhatsApp consultation (video call integration planned for future)
  - Same token mechanism as in-person
  - Medicine dispatch via courier/post
  - Courier docket sent via WhatsApp

---

## 💊 **MEDICINE & PRESCRIPTION MANAGEMENT**

### **7. Medicine Database**
- **Global Medicine Database**: Pre-populated homoeopathic medicines (maintained by super admin)
- **Clinic Customization**: Clinics can add medicines from global database or create custom ones
- **Medicine Information**: Name, potency, manufacturer, description

### **8. Prescription System**
- **Dual Templates**:
  - **Internal Template**: Full prescription with medicine names, dosage, frequency, duration
  - **Patient Template**: Same format but without medicine names (includes dosage, frequency, duration)
- **Prescription Content**:
  - Patient symptoms and diagnosis
  - Medicine details (internal only)
  - Dosage instructions
  - Frequency and duration
  - Doctor notes and advice

### **9. Inventory Management**
- **Per-Clinic Tracking**: Inventory managed separately for each clinic
- **Stock Monitoring**: Low stock alerts for clinic staff
- **Supplier Orders**: Staff can place orders and restock inventory
- **Inter-Clinic Transfers**: Inventory can be moved between clinics within same organization
- **Physical Audits**: Periodic stock verification and system updates
- **Combined Reports**: Organization admin can view combined inventory across all clinics

---

## 💰 **BILLING & PAYMENT SYSTEM**

### **10. Consultation Pricing**
- **Doctor-Specific Fees**: Each doctor sets their own consultation rates
- **Pricing Tiers**:
  - New patient vs. follow-up consultation
  - In-person vs. teleconsultation
- **Admin Control**: Only organization admin can update consultation details

### **11. Invoice Generation**
- **Invoice Components**:
  - Doctor consultation charges
  - Medicine costs
  - Courier charges (for teleconsultations)
- **Payment Processing**: Generic payment gateway integration
- **Payment Confirmation**: Medicine dispensed only after payment confirmation

---

## 📱 **COMMUNICATION & NOTIFICATIONS**

### **12. WhatsApp Integration**
- **Generic Integration**: Support for any WhatsApp Business API provider
- **Communication Types**:
  - Appointment confirmations
  - Invoice delivery
  - Courier docket sharing
  - Payment reminders
  - Prescription notifications

### **13. Email Notifications**
- **Backup Communication**: Email as backup to WhatsApp
- **Notification Types**:
  - Appointment reminders
  - Token status updates
  - Prescription ready notifications
  - Payment confirmations

---

## 📊 **REPORTING & ANALYTICS**

### **14. Business Reports**
- **Collection Reports**: Daily/weekly/monthly revenue tracking
- **Patient Statistics**: Patient demographics, visit patterns, consultation types
- **Inventory Reports**: Stock levels, consumption patterns, supplier performance
- **Clinic Performance**: Doctor productivity, patient satisfaction metrics

---

## 🔒 **SECURITY & COMPLIANCE**

### **15. Data Security**
- **Tenant Isolation**: Complete data separation between organizations
- **Role-Based Access**: Granular permissions based on user roles
- **Audit Trails**: Track all data modifications and access
- **Patient Privacy**: Secure handling of medical information

### **16. System Security**
- **JWT Authentication**: Secure token-based authentication
- **Input Validation**: Comprehensive validation for all user inputs
- **SQL Injection Prevention**: Parameterized queries only
- **HTTPS Enforcement**: Secure communication protocols

---

## 🚀 **DEPLOYMENT & SCALABILITY**

### **17. Deployment Options**
- **On-Premises**: Traditional server deployment
- **Cloud Deployment**: Azure, AWS, or other cloud providers
- **Hybrid**: Combination of on-premises and cloud services

### **18. Scalability Features**
- **Multi-Database Architecture**: 
  - Independent scaling per tenant
  - Ability to move tenant databases between servers
  - Support for different performance tiers per tenant
- **Database Management**:
  - Automated database creation
  - Migration management across tenant DBs
  - Backup/restore per tenant
- **Connection Management**:
  - Connection pooling per tenant
  - Dynamic connection string resolution
  - High availability support

---

## 📋 **FUNCTIONAL REQUIREMENTS BY USER ROLE**

### **Super Admin**
- [ ] Manage global medicine database
- [ ] System configuration management
- [ ] User account administration
- [ ] System-wide reports and analytics

### **Organization Admin**
- [ ] Manage organization settings
- [ ] Clinic and doctor management
- [ ] User role assignments
- [ ] Organization-wide reports
- [ ] Consultation fee management

### **Doctor**
- [ ] View patient appointments and queue
- [ ] Conduct patient consultations
- [ ] Create prescriptions (internal and patient templates)
- [ ] Update availability per clinic
- [ ] View patient medical history
- [ ] Manage consultation records

### **Reception Staff**
- [ ] Patient registration and management
- [ ] Appointment booking and scheduling
- [ ] Token number assignment
- [ ] Patient queue management
- [ ] Basic patient information updates

### **Pharmacy Staff**
- [ ] Medicine dispensing
- [ ] Inventory management
- [ ] Stock monitoring and alerts
- [ ] Supplier order placement
- [ ] Physical stock audits

### **Patient**
- [ ] View own medical records
- [ ] Check appointment status
- [ ] View real-time queue position
- [ ] Access prescription templates
- [ ] Receive notifications

---

## 🔧 **TECHNICAL REQUIREMENTS**

### **19. Database Design**
- **Separate Tenant Databases**: 
  - Each tenant gets dedicated database
  - Database created automatically on tenant registration
  - Standard schema applied via migrations
  - No tenant ID columns needed (physical isolation)
- **Connection Management**:
  - Dynamic connection string resolution
  - Connection pooling per tenant
  - Database name template: `ClinicCare_{tenant}`
- **Entity Relationships**: Proper foreign key constraints and indexes
- **Audit Fields**: Created/Updated timestamps and user tracking
- **Soft Deletes**: Data retention and recovery capabilities

### **20. API Design**
- **RESTful Endpoints**: Standard HTTP methods and status codes
- **Versioning**: API versioning strategy
- **Rate Limiting**: Protection against abuse
- **Error Handling**: Consistent error response format

### **21. Frontend Requirements**
- **Responsive Design**: Mobile, tablet, and desktop compatibility
- **Real-Time Updates**: Live queue position and status updates
- **Offline Support**: Basic functionality when offline
- **Accessibility**: WCAG compliance for medical software

---

## 📱 **INTEGRATION REQUIREMENTS**

### **22. External Services**
- **WhatsApp Business API**: Generic integration for multiple providers
- **Payment Gateways**: Generic integration for various payment processors
- **Email Services**: SMTP integration for notifications
- **SMS Services**: Optional SMS notifications

### **23. Data Import/Export**
- **Patient Data**: CSV/Excel import/export
- **Medicine Database**: Bulk medicine data management
- **Reports**: PDF/Excel report generation
- **Backup/Restore**: Database backup and recovery

---

## 🧪 **TESTING REQUIREMENTS**

### **24. Testing Strategy**
- **Unit Testing**: Business logic and data access testing
- **Integration Testing**: API endpoint and database testing
- **Multi-Tenant Testing**: Tenant isolation verification
- **Security Testing**: Authentication and authorization testing
- **Performance Testing**: Load testing for multi-tenant scenarios

---

## 📚 **DELIVERABLES**

### **Phase 1: Core Backend**
- [ ] Multi-tenant database schema
- [ ] Authentication and authorization system
- [ ] Basic CRUD operations for all entities
- [ ] Multi-tenant data isolation
- [ ] Basic API endpoints

### **Phase 2: Business Logic**
- [ ] Appointment and token management
- [ ] Prescription system
- [ ] Inventory management
- [ ] Billing and invoicing
- [ ] User role management

### **Phase 3: Frontend Development**
- [ ] User authentication flows
- [ ] Role-based dashboards
- [ ] Real-time queue management
- [ ] Responsive design implementation

### **Phase 4: Advanced Features**
- [ ] WhatsApp integration
- [ ] Payment gateway integration
- [ ] Reporting and analytics
- [ ] Communication system

---

## ⚠️ **CONSTRAINTS & ASSUMPTIONS**

### **Technical Constraints**
- **SQL Server Compatibility**: Minimum SQL Server 2016 required
- **Browser Support**: Modern browsers (Chrome, Firefox, Safari, Edge)
- **Mobile Support**: Responsive design for mobile devices
- **Database Management**: 
  - Requires permissions to create new databases
  - Sufficient disk space for multiple databases
  - Regular maintenance window for each tenant

### **Business Constraints**
- **Data Privacy**: Medical data must be handled securely
- **Compliance**: Follow local healthcare data regulations
- **Performance**: System must handle multiple concurrent users per clinic
- **Storage**: Plan for database growth per tenant

### **Assumptions**
- **Database Creation**: System has permissions to create new databases
- **Storage Capacity**: Sufficient storage for multiple tenant databases
- **Backup Management**: Separate backup strategy per tenant database
- **Internet Connectivity**: Stable internet connection for cloud features
- **User Training**: Staff will receive basic system training
- **Data Migration**: Existing patient data can be imported to new tenant DB

---

## 🎯 **SUCCESS CRITERIA**

### **Functional Success**
- [ ] All user roles can perform their designated functions
- [ ] Multi-tenant isolation works correctly
- [ ] Real-time queue updates function properly
- [ ] Prescription system generates correct templates
- [ ] Inventory management tracks stock accurately

### **Technical Success**
- [ ] System responds within 2 seconds for most operations
- [ ] Supports 100+ concurrent users per organization
- [ ] 99.9% uptime for production deployments
- [ ] Secure data transmission and storage
- [ ] Comprehensive error handling and logging

### **Business Success**
- [ ] Reduces patient wait times
- [ ] Improves inventory management efficiency
- [ ] Streamlines billing and payment processes
- [ ] Enhances patient experience and satisfaction
- [ ] Provides actionable business insights

---

## 📞 **SUPPORT & MAINTENANCE**

### **25. Support Requirements**
- **User Documentation**: Comprehensive user guides and help system
- **Technical Documentation**: API documentation and system architecture
- **Training Materials**: Video tutorials and training guides
- **Support System**: Help desk and issue tracking

### **26. Maintenance Requirements**
- **Regular Updates**: Security patches and feature updates
- **Backup Procedures**: Automated backup and recovery processes
- **Monitoring**: System health monitoring and alerting
- **Performance Optimization**: Continuous performance improvement

---

**Note**: This requirements specification serves as the foundation for developing the ClinicCare system. All development should follow the established architecture patterns and maintain the security and multi-tenant isolation requirements throughout the implementation process.

