import React, { useState, useEffect } from 'react';
import { useAuth } from '../hooks/useAuth';
import Card from '../components/common/Card';
import QuickActionButton from '../components/common/QuickActionButton';
import LoadingSpinner from '../components/common/LoadingSpinner';
import { dashboardService } from '../services/dashboardService';

const StaffDashboard = () => {
  const { user } = useAuth();
  const [stats, setStats] = useState({
    totalStudents: 0,
    totalPayments: 0,
    totalRevenue: 0,
    activeStudents: 0,
    inactiveStudents: 0,
    recentPayments: [],
    pendingTasks: []
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchStaffStats = async () => {
      try {
        setLoading(true);
        setError(null);
        
        const response = await dashboardService.getStaffStats();
        
        if (response.success && response.data) {
          setStats({
            totalStudents: response.data.totalStudents || 0,
            totalPayments: response.data.totalPayments || 0,
            totalRevenue: response.data.totalRevenue || 0,
            activeStudents: response.data.activeStudents || 0,
            inactiveStudents: response.data.inactiveStudents || 0,
            recentPayments: response.data.recentPayments || [],
            pendingTasks: response.data.pendingTasks || []
          });
        } else {
          setError(response.message || 'Failed to load dashboard data');
        }
      } catch (error) {
        console.error('Error fetching staff stats:', error);
        setError('Failed to load dashboard data. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    fetchStaffStats();
  }, []);

  if (loading) {
    return <LoadingSpinner size="large" text="Loading staff dashboard..." />;
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-50 via-amber-50/30 to-orange-50/30 p-6">
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
              className="px-6 py-2 bg-amber-600 text-white rounded-lg hover:bg-amber-700 transition-colors"
            >
              Retry
            </button>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-amber-50/30 to-orange-50/30 p-6 space-y-8">
      {/* Header */}
      <Card variant="elevated" className="p-8">
        <div className="text-center">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-amber-600 to-orange-600 bg-clip-text text-transparent mb-3">
            Staff Dashboard
          </h1>
          <p className="text-lg text-gray-600">
            Welcome back, <span className="font-semibold text-gray-800">{user?.name || 'Staff Member'}</span>. Here's your daily overview.
          </p>
        </div>
      </Card>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-green-100 to-green-200 text-green-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Total Payments</p>
              <p className="text-3xl font-bold text-gray-900">{stats.totalPayments}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-yellow-100 to-yellow-200 text-yellow-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Active Students</p>
              <p className="text-3xl font-bold text-gray-900">{stats.activeStudents}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-blue-100 to-blue-200 text-blue-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Total Students</p>
              <p className="text-3xl font-bold text-gray-900">{stats.totalStudents}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-purple-100 to-purple-200 text-purple-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Total Revenue</p>
              <p className="text-3xl font-bold text-gray-900">${stats.totalRevenue.toLocaleString()}</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <Card variant="elevated" className="p-6">
          <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
            <span className="w-2 h-8 bg-gradient-to-b from-amber-500 to-orange-500 rounded-full mr-3"></span>
            Quick Actions
          </h3>
          <div className="space-y-4">
            <QuickActionButton
              icon={<svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
              </svg>}
              variant="primary"
              className="group"
            >
              Process Payment
            </QuickActionButton>
            
            <QuickActionButton
              icon={<svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>}
              variant="secondary"
              className="group"
            >
              Student Search
            </QuickActionButton>
            
            <QuickActionButton
              icon={<svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>}
              variant="secondary"
              className="group"
            >
              Generate Receipt
            </QuickActionButton>
            
            <QuickActionButton
              icon={<svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>}
              variant="secondary"
              className="group"
            >
              Help & Support
            </QuickActionButton>
          </div>
        </Card>

        <Card variant="elevated" className="p-6">
          <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
            <span className="w-2 h-8 bg-gradient-to-b from-green-500 to-blue-500 rounded-full mr-3"></span>
            Recent Activity
          </h3>
          <div className="space-y-4">
            <div className="flex items-center text-sm p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <div className="w-3 h-3 bg-gradient-to-r from-green-400 to-green-500 rounded-full mr-4 animate-pulse"></div>
              <span className="text-gray-700 flex-1">Payment processed for John Doe</span>
              <span className="text-gray-400 font-medium">2 min ago</span>
            </div>
            <div className="flex items-center text-sm p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <div className="w-3 h-3 bg-gradient-to-r from-blue-400 to-blue-500 rounded-full mr-4 animate-pulse"></div>
              <span className="text-gray-700 flex-1">Student record updated</span>
              <span className="text-gray-400 font-medium">10 min ago</span>
            </div>
            <div className="flex items-center text-sm p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <div className="w-3 h-3 bg-gradient-to-r from-yellow-400 to-yellow-500 rounded-full mr-4 animate-pulse"></div>
              <span className="text-gray-700 flex-1">Receipt generated for Jane Smith</span>
              <span className="text-gray-400 font-medium">15 min ago</span>
            </div>
            <div className="flex items-center text-sm p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <div className="w-3 h-3 bg-gradient-to-r from-purple-400 to-purple-500 rounded-full mr-4 animate-pulse"></div>
              <span className="text-gray-700 flex-1">Payment query resolved</span>
              <span className="text-gray-400 font-medium">30 min ago</span>
            </div>
          </div>
        </Card>
      </div>

      {/* Pending Payments */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
          <span className="w-2 h-8 bg-gradient-to-b from-red-500 to-pink-500 rounded-full mr-3"></span>
          Pending Payments
        </h3>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gradient-to-r from-gray-50 to-gray-100/50">
              <tr>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Student
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Payment Type
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Amount
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Action
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-100">
              <tr className="hover:bg-gray-50/50 transition-colors duration-200">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  Alice Johnson
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  Tuition Fee
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                  $2,500
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span className="px-3 py-1 text-xs font-semibold bg-gradient-to-r from-yellow-100 to-yellow-200 text-yellow-800 rounded-full">
                    Pending
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <button className="text-blue-600 hover:text-blue-900 transition-colors duration-200">Process</button>
                </td>
              </tr>
              <tr className="hover:bg-gray-50/50 transition-colors duration-200">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  Bob Wilson
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  Library Fee
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                  $150
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span className="px-3 py-1 text-xs font-semibold bg-gradient-to-r from-yellow-100 to-yellow-200 text-yellow-800 rounded-full">
                    Pending
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <button className="text-blue-600 hover:text-blue-900 transition-colors duration-200">Process</button>
                </td>
              </tr>
              <tr className="hover:bg-gray-50/50 transition-colors duration-200">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  Carol Davis
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  Lab Fee
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                  $200
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span className="px-3 py-1 text-xs font-semibold bg-gradient-to-r from-yellow-100 to-yellow-200 text-yellow-800 rounded-full">
                    Pending
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <button className="text-blue-600 hover:text-blue-900 transition-colors duration-200">Process</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
};

export default StaffDashboard; 