import React, { useState, useEffect } from 'react';
import { useAuth } from '../hooks/useAuth';
import Card from '../components/common/Card';
import QuickActionButton from '../components/common/QuickActionButton';
import LoadingSpinner from '../components/common/LoadingSpinner';

const ManagerDashboard = () => {
  const { user } = useAuth();
  const [stats, setStats] = useState({
    totalStudents: 0,
    totalPayments: 0,
    pendingApprovals: 0,
    monthlyRevenue: 0
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Simulate loading stats
    setTimeout(() => {
      setStats({
        totalStudents: 89,
        totalPayments: 67,
        pendingApprovals: 12,
        monthlyRevenue: 45000
      });
      setLoading(false);
    }, 1000);
  }, []);

  if (loading) {
    return <LoadingSpinner size="large" text="Loading manager dashboard..." />;
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-emerald-50/30 to-teal-50/30 p-6 space-y-8">
      {/* Header */}
      <Card variant="elevated" className="p-8">
        <div className="text-center">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-emerald-600 to-teal-600 bg-clip-text text-transparent mb-3">
            Manager Dashboard
          </h1>
          <p className="text-lg text-gray-600">
            Welcome back, <span className="font-semibold text-gray-800">{user?.name || 'Manager'}</span>. Here's your department overview.
          </p>
        </div>
      </Card>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
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
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Pending Approvals</p>
              <p className="text-3xl font-bold text-gray-900">{stats.pendingApprovals}</p>
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
              <p className="text-sm font-medium text-gray-600 mb-1">Monthly Revenue</p>
              <p className="text-3xl font-bold text-gray-900">${stats.monthlyRevenue.toLocaleString()}</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Quick Actions */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <Card variant="elevated" className="p-6">
          <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
            <span className="w-2 h-8 bg-gradient-to-b from-emerald-500 to-teal-500 rounded-full mr-3"></span>
            Quick Actions
          </h3>
          <div className="space-y-4">
            <QuickActionButton
              icon={<svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>}
              variant="primary"
              className="group"
            >
              Review Reports
            </QuickActionButton>
            
            <QuickActionButton
              icon={<svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>}
              variant="secondary"
              className="group"
            >
              Approve Payments
            </QuickActionButton>
            
            <QuickActionButton
              icon={<svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>}
              variant="secondary"
              className="group"
            >
              Manage Staff
            </QuickActionButton>
            
            <QuickActionButton
              icon={<svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>}
              variant="secondary"
              className="group"
            >
              View Analytics
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
              <span className="text-gray-700 flex-1">Payment approved for Student ID 2024001</span>
              <span className="text-gray-400 font-medium">5 min ago</span>
            </div>
            <div className="flex items-center text-sm p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <div className="w-3 h-3 bg-gradient-to-r from-blue-400 to-blue-500 rounded-full mr-4 animate-pulse"></div>
              <span className="text-gray-700 flex-1">New staff member added</span>
              <span className="text-gray-400 font-medium">15 min ago</span>
            </div>
            <div className="flex items-center text-sm p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <div className="w-3 h-3 bg-gradient-to-r from-yellow-400 to-yellow-500 rounded-full mr-4 animate-pulse"></div>
              <span className="text-gray-700 flex-1">Monthly report generated</span>
              <span className="text-gray-400 font-medium">1 hour ago</span>
            </div>
            <div className="flex items-center text-sm p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <div className="w-3 h-3 bg-gradient-to-r from-purple-400 to-purple-500 rounded-full mr-4 animate-pulse"></div>
              <span className="text-gray-700 flex-1">System maintenance completed</span>
              <span className="text-gray-400 font-medium">2 hours ago</span>
            </div>
          </div>
        </Card>
      </div>

      {/* Pending Approvals */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
          <span className="w-2 h-8 bg-gradient-to-b from-orange-500 to-red-500 rounded-full mr-3"></span>
          Pending Approvals
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
                  Date
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Action
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-100">
              <tr className="hover:bg-gray-50/50 transition-colors duration-200">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  John Doe
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  Tuition Fee
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                  $2,500
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  2024-01-15
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <button className="text-green-600 hover:text-green-900 mr-3 transition-colors duration-200">Approve</button>
                  <button className="text-red-600 hover:text-red-900 transition-colors duration-200">Reject</button>
                </td>
              </tr>
              <tr className="hover:bg-gray-50/50 transition-colors duration-200">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  Jane Smith
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  Library Fee
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                  $150
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  2024-01-14
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <button className="text-green-600 hover:text-green-900 mr-3 transition-colors duration-200">Approve</button>
                  <button className="text-red-600 hover:text-red-900 transition-colors duration-200">Reject</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
};

export default ManagerDashboard; 