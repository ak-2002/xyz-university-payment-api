import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import Card from '../components/common/Card';
import QuickActionButton from '../components/common/QuickActionButton';
import NotificationCard from '../components/common/NotificationCard';
import LoadingSpinner from '../components/common/LoadingSpinner';
import { dashboardService } from '../services/dashboardService';
import { paymentService } from '../services/paymentService';

const StudentDashboard = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [studentData, setStudentData] = useState({
    userRole: '',
    currentUser: '',
    userId: '',
    userRoles: [],
    message: '',
    academicInfo: {},
    financialInfo: {},
    systemInfo: {}
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [payments, setPayments] = useState([]);

  useEffect(() => {
    const fetchStudentStats = async () => {
      try {
        setLoading(true);
        setError(null);
        
        console.log('StudentDashboard: Starting to fetch student stats...');
        const response = await dashboardService.getStudentStats();
        console.log('StudentDashboard: Received response:', response);
        
        if (response.success && response.data) {
          console.log('StudentDashboard: Setting student data with:', response.data);
          setStudentData({
            userRole: response.data.userRole || '',
            currentUser: response.data.currentUser || '',
            userId: response.data.userId || '',
            userRoles: response.data.userRoles || [],
            message: response.data.message || '',
            academicInfo: response.data.academicInfo || {},
            financialInfo: response.data.financialInfo || {},
            systemInfo: response.data.systemInfo || {}
          });
        } else {
          console.log('StudentDashboard: Response not successful:', response);
          setError(response.message || 'Failed to load student data');
        }
      } catch (error) {
        console.error('StudentDashboard: Error fetching student stats:', error);
        setError('Failed to load student data. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    fetchStudentStats();
    loadStudentPayments();
  }, []);

  const loadStudentPayments = async () => {
    try {
      const studentNumber = user?.name || '';
      const response = await paymentService.getPaymentsByStudent(studentNumber);
      setPayments(response.data || []);
    } catch (err) {
      setPayments([]);
    }
  };

  // Quick action handlers
  const handleMakePayment = () => {
    navigate('/payments');
  };

  const handleViewPaymentHistory = () => {
    navigate('/payments');
  };

  const handleDownloadReceipts = () => {
    // For now, navigate to payments page where they can view receipts
    navigate('/payments');
  };

  const handleRequestSupport = () => {
    // For now, show an alert - could be enhanced with a support form
    alert('Support request feature coming soon! Please contact the administration office for immediate assistance.');
  };

  // Calculate totalPaid from payments
  const totalPaid = payments.reduce((sum, payment) => sum + (payment.amountPaid || 0), 0);
  // Use backend outstanding balance if available, else fallback
  const outstandingBalance = studentData.financialInfo?.balance !== undefined && studentData.financialInfo?.balance !== null
    ? studentData.financialInfo.balance
    : Math.max(0, 5000 - totalPaid);

  if (loading) {
    return <LoadingSpinner size="large" text="Loading student dashboard..." />;
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50/30 to-purple-50/30 p-6">
        <Card variant="elevated" className="p-8">
          <div className="text-center">
            <div className="text-red-600 mb-4">
              <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Error Loading Dashboard</h2>
            <p className="text-gray-600 mb-4">{error}</p>
            <button 
              onClick={() => window.location.reload()} 
              className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              Retry
            </button>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50/30 to-purple-50/30 p-6 space-y-8">
      {/* Header */}
      <Card variant="elevated" className="p-8">
        <div className="text-center">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent mb-3">
            Student Portal
          </h1>
          <p className="text-lg text-gray-600 mb-6">
            Welcome back, <span className="font-semibold text-gray-800">{studentData.currentUser || user?.name || 'Student'}</span>. Here's your academic and financial overview.
          </p>
          <div className="inline-block p-4 bg-gradient-to-r from-blue-100 to-purple-100 rounded-2xl border border-blue-200/50">
            <p className="text-sm text-blue-800 font-medium">
              <span className="text-blue-600 font-bold">User ID:</span> {studentData.userId}
            </p>
            {studentData.userRole && (
              <p className="text-sm text-blue-800 font-medium mt-1">
                <span className="text-blue-600 font-bold">Role:</span> {studentData.userRole}
              </p>
            )}
          </div>
        </div>
      </Card>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <Card variant="elevated" className="p-6 group cursor-pointer hover:shadow-lg transition-all duration-300">
          <div className="flex items-center">
            <div className="p-3 rounded-2xl bg-gradient-to-br from-blue-100 to-blue-200 text-blue-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">View Profile</p>
              <p className="text-lg font-bold text-gray-900">My Details</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group cursor-pointer hover:shadow-lg transition-all duration-300">
          <div className="flex items-center">
            <div className="p-3 rounded-2xl bg-gradient-to-br from-green-100 to-green-200 text-green-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Make Payment</p>
              <p className="text-lg font-bold text-gray-900">Pay Fees</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group cursor-pointer hover:shadow-lg transition-all duration-300">
          <div className="flex items-center">
            <div className="p-3 rounded-2xl bg-gradient-to-br from-purple-100 to-purple-200 text-purple-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">View History</p>
              <p className="text-lg font-bold text-gray-900">Payment History</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group cursor-pointer hover:shadow-lg transition-all duration-300">
          <div className="flex items-center">
            <div className="p-3 rounded-2xl bg-gradient-to-br from-orange-100 to-orange-200 text-orange-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="ml-4">
              <p className="text-sm font-medium text-gray-600">Get Help</p>
              <p className="text-lg font-bold text-gray-900">Support</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Academic Overview */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-8">
        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-blue-100 to-blue-200 text-blue-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Current Semester</p>
              <p className="text-3xl font-bold text-gray-900">{studentData.academicInfo?.currentSemester || 'Summer 2025'}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-green-100 to-green-200 text-green-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Enrollment Status</p>
              <p className="text-3xl font-bold text-gray-900">{studentData.academicInfo?.enrollmentStatus || 'Active'}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-purple-100 to-purple-200 text-purple-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Academic Standing</p>
              <p className="text-3xl font-bold text-gray-900">{studentData.academicInfo?.academicStanding || 'Good Standing'}</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Financial Overview */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-8">
        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-red-100 to-red-200 text-red-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Outstanding Balance</p>
              <p className="text-3xl font-bold text-gray-900">${outstandingBalance.toLocaleString()}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-green-100 to-green-200 text-green-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-green-600 mb-1">Total Paid</p>
              <p className="text-3xl font-bold text-gray-900">${totalPaid.toLocaleString()}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-yellow-100 to-yellow-200 text-yellow-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Next Payment Due</p>
              <p className="text-xl font-bold text-gray-900">
                {studentData.financialInfo?.nextPaymentDue ? new Date(studentData.financialInfo.nextPaymentDue).toLocaleDateString() : 'N/A'}
              </p>
            </div>
          </div>
        </Card>
      </div>

      {/* Recent Payments */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
          <span className="w-2 h-8 bg-gradient-to-b from-purple-500 to-pink-500 rounded-full mr-3"></span>
          Recent Payments
        </h3>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gradient-to-r from-gray-50 to-gray-100/50">
              <tr>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Date
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Description
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Amount
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Status
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-100">
              {studentData.financialInfo?.recentPayments && studentData.financialInfo.recentPayments.length > 0 ? (
                studentData.financialInfo.recentPayments.map((payment, index) => (
                  <tr key={payment.id || index} className="hover:bg-gray-50/50 transition-colors duration-200">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {payment.date}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {payment.description}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                      ${payment.amount.toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="px-3 py-1 text-xs font-semibold bg-gradient-to-r from-green-100 to-green-200 text-green-800 rounded-full">
                        {payment.status}
                      </span>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan="4" className="px-6 py-8 text-center text-gray-500">
                    <div className="flex flex-col items-center">
                      <svg className="w-12 h-12 text-gray-300 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                      </svg>
                      <p className="text-sm">No recent payments found</p>
                      <p className="text-xs text-gray-400 mt-1">Your payment history will appear here</p>
                    </div>
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Notifications */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
          <span className="w-2 h-8 bg-gradient-to-b from-orange-500 to-red-500 rounded-full mr-3"></span>
          Important Notices
        </h3>
        <div className="space-y-4">
          <NotificationCard
            icon={<svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>}
            variant="warning"
          >
            <p className="text-base font-bold mb-1">Payment Due Soon</p>
            <p className="text-sm">Your next payment of ${studentData.financialInfo?.nextPaymentDueAmount || 0} is due on {studentData.financialInfo?.nextPaymentDue ? new Date(studentData.financialInfo.nextPaymentDue).toLocaleDateString() : 'N/A'}</p>
          </NotificationCard>
          
          <NotificationCard
            icon={<svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>}
            variant="info"
          >
            <p className="text-base font-bold mb-1">Academic Calendar</p>
            <p className="text-sm">Spring semester registration opens on February 1st, 2024</p>
          </NotificationCard>
        </div>
      </Card>
    </div>
  );
};

export default StudentDashboard; 