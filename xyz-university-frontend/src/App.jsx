import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './hooks/useAuth';
import Layout from './components/layout/Layout';
import LoginPage from './pages/LoginPage';
import AdminDashboard from './pages/AdminDashboard';
import ManagerDashboard from './pages/ManagerDashboard';
import StaffDashboard from './pages/StaffDashboard';
import StudentDashboard from './pages/StudentDashboard';
import StudentManagement from './pages/StudentManagement';
import StudentProfile from './pages/StudentProfile';
import PaymentManagement from './pages/PaymentManagement';
import StudentPayments from './pages/StudentPayments';
import RoleManagement from './pages/RoleManagement';
import Reports from './pages/Reports';
import TestAPI from './pages/TestAPI';
import LoadingSpinner from './components/common/LoadingSpinner';
import FeeManagementDashboard from './pages/FeeManagementDashboard';
import FeeStructureManager from './pages/FeeStructureManager';

// Placeholder components for now - we'll create these in the next steps
const StudentsPage = () => {
  const { getPrimaryRole } = useAuth();
  const userRole = getPrimaryRole();
  
  // Show StudentProfile for students, StudentManagement for admin/staff
  if (userRole === 'student') {
    return <StudentProfile />;
  }
  return <StudentManagement />;
};

const PaymentsPage = () => {
  const { getPrimaryRole } = useAuth();
  const userRole = getPrimaryRole();
  
  // Show StudentPayments for students, PaymentManagement for admin/staff
  if (userRole === 'student') {
    return <StudentPayments />;
  }
  return <PaymentManagement />;
};

const ReportsPage = () => <Reports />;
const TestAPIPage = () => <TestAPI />;

// Protected route component
const ProtectedRoute = ({ children }) => {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <LoadingSpinner size="large" text="Loading..." />
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return children;
};

// Role-based dashboard component
const RoleBasedDashboard = () => {
  const { getPrimaryRole } = useAuth();
  
  const userRole = getPrimaryRole();

  switch (userRole) {
    case 'admin':
      return <AdminDashboard />;
    case 'manager':
      return <ManagerDashboard />;
    case 'staff':
      return <StaffDashboard />;
    case 'student':
    default:
      return <StudentDashboard />;
  }
};

// App content component
const AppContent = () => {
  return (
    <Router>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <Layout>
                <RoleBasedDashboard />
              </Layout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/students"
          element={
            <ProtectedRoute>
              <Layout>
                <StudentsPage />
              </Layout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/payments"
          element={
            <ProtectedRoute>
              <Layout>
                <PaymentsPage />
              </Layout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/reports"
          element={
            <ProtectedRoute>
              <Layout>
                <ReportsPage />
              </Layout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/role-management"
          element={
            <ProtectedRoute>
              <Layout>
                <RoleManagement />
              </Layout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/test-api"
          element={
            <ProtectedRoute>
              <Layout>
                <TestAPIPage />
              </Layout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/fee-management"
          element={
            <ProtectedRoute>
              <Layout>
                <FeeManagementDashboard />
              </Layout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/fee-structures"
          element={
            <ProtectedRoute>
              <Layout>
                <FeeStructureManager />
              </Layout>
            </ProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </Router>
  );
};

// Main App component
function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;
