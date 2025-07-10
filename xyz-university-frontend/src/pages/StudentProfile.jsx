import React, { useState, useEffect } from 'react';
import { useAuth } from '../hooks/useAuth';
import Card from '../components/common/Card';
import LoadingSpinner from '../components/common/LoadingSpinner';
import { dashboardService } from '../services/dashboardService';

const StudentProfile = () => {
  const { user } = useAuth();
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

  useEffect(() => {
    const fetchStudentData = async () => {
      try {
        setLoading(true);
        setError(null);
        
        console.log('StudentProfile: Starting to fetch student data...');
        const response = await dashboardService.getStudentStats();
        console.log('StudentProfile: Received response:', response);
        
        if (response.success && response.data) {
          console.log('StudentProfile: Setting student data with:', response.data);
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
          console.log('StudentProfile: Response not successful:', response);
          setError(response.message || 'Failed to load student data');
        }
      } catch (error) {
        console.error('StudentProfile: Error fetching student data:', error);
        setError('Failed to load student data. Please try again.');
      } finally {
        setLoading(false);
      }
    };

    fetchStudentData();
  }, []);

  if (loading) {
    return <LoadingSpinner size="large" text="Loading student profile..." />;
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
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Error Loading Profile</h2>
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
          <div className="w-24 h-24 mx-auto mb-6 bg-gradient-to-br from-blue-500 to-purple-600 rounded-full flex items-center justify-center">
            <span className="text-3xl font-bold text-white">
              {studentData.currentUser ? studentData.currentUser.charAt(0).toUpperCase() : 'S'}
            </span>
          </div>
          <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent mb-3">
            Student Profile
          </h1>
          <p className="text-lg text-gray-600 mb-6">
            Welcome, <span className="font-semibold text-gray-800">{studentData.currentUser || user?.name || 'Student'}</span>
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

      {/* Personal Information */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        <Card variant="elevated" className="p-6">
          <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
            <span className="w-2 h-8 bg-gradient-to-b from-blue-500 to-purple-500 rounded-full mr-3"></span>
            Personal Information
          </h3>
          <div className="space-y-4">
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">Full Name:</span>
              <span className="font-bold text-gray-900">{studentData.currentUser || 'Not specified'}</span>
            </div>
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">Email:</span>
              <span className="font-bold text-gray-900">{user?.email || 'Not specified'}</span>
            </div>
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">Phone Number:</span>
              <span className="font-bold text-gray-900">+254 700 000 000</span>
            </div>
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">Address:</span>
              <span className="font-bold text-gray-900">Nairobi, Kenya</span>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6">
          <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
            <span className="w-2 h-8 bg-gradient-to-b from-green-500 to-blue-500 rounded-full mr-3"></span>
            Academic Information
          </h3>
          <div className="space-y-4">
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">Current Semester:</span>
              <span className="font-bold text-gray-900">{studentData.academicInfo?.currentSemester || 'Not specified'}</span>
            </div>
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">Enrollment Status:</span>
              <span className={`px-3 py-1 text-sm font-semibold rounded-full ${
                studentData.academicInfo?.enrollmentStatus === 'Active'
                  ? 'bg-gradient-to-r from-green-100 to-green-200 text-green-800' 
                  : 'bg-gradient-to-r from-red-100 to-red-200 text-red-800'
              }`}>
                {studentData.academicInfo?.enrollmentStatus || 'Unknown'}
              </span>
            </div>
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">Academic Standing:</span>
              <span className="font-bold text-gray-900">{studentData.academicInfo?.academicStanding || 'Not specified'}</span>
            </div>
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">Program:</span>
              <span className="font-bold text-gray-900">Computer Science</span>
            </div>
          </div>
        </Card>
      </div>

      {/* Financial Summary */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
          <span className="w-2 h-8 bg-gradient-to-b from-purple-500 to-pink-500 rounded-full mr-3"></span>
          Financial Summary
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="p-4 bg-gradient-to-r from-red-50 to-red-100 rounded-xl border border-red-200">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-red-200 text-red-600">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-red-600">Outstanding Balance</p>
                <p className="text-2xl font-bold text-gray-900">${(studentData.financialInfo?.balance || 0).toLocaleString()}</p>
              </div>
            </div>
          </div>

          <div className="p-4 bg-gradient-to-r from-green-50 to-green-100 rounded-xl border border-green-200">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-green-200 text-green-600">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-green-600">Total Paid</p>
                <p className="text-2xl font-bold text-gray-900">${studentData.financialInfo?.totalPaid || 0}</p>
              </div>
            </div>
          </div>

          <div className="p-4 bg-gradient-to-r from-yellow-50 to-yellow-100 rounded-xl border border-yellow-200">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-yellow-200 text-yellow-600">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-yellow-600">Next Payment Due</p>
                <p className="text-lg font-bold text-gray-900">
                  {studentData.financialInfo?.nextPaymentDue ? new Date(studentData.financialInfo.nextPaymentDue).toLocaleDateString() : 'N/A'}
                </p>
              </div>
            </div>
          </div>
        </div>
      </Card>

      {/* Account Information */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
          <span className="w-2 h-8 bg-gradient-to-b from-orange-500 to-red-500 rounded-full mr-3"></span>
          Account Information
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-4">
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">Account Status:</span>
              <span className="px-3 py-1 text-sm font-semibold bg-gradient-to-r from-green-100 to-green-200 text-green-800 rounded-full">
                Active
              </span>
            </div>
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">Last Login:</span>
              <span className="font-bold text-gray-900">
                {studentData.systemInfo?.lastLogin ? new Date(studentData.systemInfo.lastLogin).toLocaleString() : 'N/A'}
              </span>
            </div>
          </div>
          <div className="space-y-4">
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">User Roles:</span>
              <span className="font-bold text-gray-900">
                {studentData.userRoles?.join(', ') || 'Student'}
              </span>
            </div>
            <div className="flex justify-between items-center p-3 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
              <span className="text-gray-600 font-medium">Profile Updated:</span>
              <span className="font-bold text-gray-900">Today</span>
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
};

export default StudentProfile; 