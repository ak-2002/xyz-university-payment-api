import React, { useState, useEffect } from 'react';
import { useAuth } from '../hooks/useAuth';
import Card from '../components/common/Card';
import QuickActionButton from '../components/common/QuickActionButton';
import LoadingSpinner from '../components/common/LoadingSpinner';

const StaffDashboard = () => {
  const { user } = useAuth();
  const [stats, setStats] = useState({
    processedPayments: 0,
    pendingPayments: 0,
    totalStudents: 0,
    dailyRevenue: 0
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Simulate loading stats
    setTimeout(() => {
      setStats({
        processedPayments: 23,
        pendingPayments: 8,
        totalStudents: 156,
        dailyRevenue: 8500
      });
      setLoading(false);
    }, 1000);
  }, []);

  if (loading) {
    return <LoadingSpinner size="large" text="Loading staff dashboard..." />;
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
              <p className="text-sm font-medium text-gray-600 mb-1">Processed Today</p>
              <p className="text-3xl font-bold text-gray-900">{stats.processedPayments}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-yellow-100 to-yellow-200 text-yellow-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Pending Payments</p>
              <p className="text-3xl font-bold text-gray-900">{stats.pendingPayments}</p>
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
              <p className="text-sm font-medium text-gray-600 mb-1">Daily Revenue</p>
              <p className="text-3xl font-bold text-gray-900">${stats.dailyRevenue.toLocaleString()}</p>
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