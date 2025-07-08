import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './hooks/useAuth';
import Layout from './components/layout/Layout';
import LoginPage from './pages/LoginPage';
import AdminDashboard from './pages/AdminDashboard';
import ManagerDashboard from './pages/ManagerDashboard';
import StaffDashboard from './pages/StaffDashboard';
import StudentDashboard from './pages/StudentDashboard';
import LoadingSpinner from './components/common/LoadingSpinner';

// Placeholder components for now - we'll create these in the next steps
const StudentsPage = () => (
  <div className="bg-white shadow rounded-lg p-6">
    <h1 className="text-2xl font-bold text-gray-900 mb-4">Students</h1>
    <p className="text-gray-600">Student management interface coming soon...</p>
  </div>
);

const PaymentsPage = () => (
  <div className="bg-white shadow rounded-lg p-6">
    <h1 className="text-2xl font-bold text-gray-900 mb-4">Payments</h1>
    <p className="text-gray-600">Payment management interface coming soon...</p>
  </div>
);

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
