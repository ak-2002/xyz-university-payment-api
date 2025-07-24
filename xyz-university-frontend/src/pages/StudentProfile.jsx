import React, { useState, useEffect } from 'react';
import { useAuth } from '../hooks/useAuth';
import Card from '../components/common/Card';
import LoadingSpinner from '../components/common/LoadingSpinner';
import { dashboardService } from '../services/dashboardService';
import { feeManagementService } from '../services/feeManagementService';

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
  const [selectedSemester, setSelectedSemester] = useState('');
  const [selectedAcademicYear, setSelectedAcademicYear] = useState('');
  const [feeStructures, setFeeStructures] = useState([]);
  const [loadingFeeData, setLoadingFeeData] = useState(false);

  const availableSemesters = feeManagementService.getAvailableSemesters();
  const availableAcademicYears = feeManagementService.getAvailableAcademicYears();

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

          // Set default semester and academic year based on current academic info
          if (response.data.academicInfo?.currentSemester) {
            setSelectedSemester(response.data.academicInfo.currentSemester);
          } else {
            setSelectedSemester('Fall'); // Default to Fall
          }
          
          if (response.data.academicInfo?.academicYear) {
            setSelectedAcademicYear(response.data.academicInfo.academicYear);
          } else {
            setSelectedAcademicYear(new Date().getFullYear().toString());
          }
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

  // Fetch fee structures when semester or academic year changes
  useEffect(() => {
    if (selectedSemester && selectedAcademicYear) {
      fetchFeeStructures();
    }
  }, [selectedSemester, selectedAcademicYear]);

  const fetchFeeStructures = async () => {
    try {
      setLoadingFeeData(true);
      console.log('Fetching fee structures for:', selectedAcademicYear, selectedSemester);
      console.log('Current user role:', studentData.userRole);
      const structures = await feeManagementService.getFeeStructuresBySemester(selectedAcademicYear, selectedSemester);
      console.log('Received fee structures:', structures);
      setFeeStructures(structures);
    } catch (error) {
      console.error('Error fetching fee structures:', error);
      setFeeStructures([]);
    } finally {
      setLoadingFeeData(false);
    }
  };

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
            <div className="flex justify-between items-center p-3 b g-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl">
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

      {/* Enhanced Academic Information */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
          <span className="w-2 h-8 bg-gradient-to-b from-blue-500 to-indigo-500 rounded-full mr-3"></span>
          Academic Information
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="p-4 bg-gradient-to-r from-blue-50 to-blue-100 rounded-xl border border-blue-200">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-blue-200 text-blue-600">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.246 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-blue-600">Current Semester</p>
                <p className="text-2xl font-bold text-gray-900">{studentData.academicInfo?.currentSemester || 'Summer 2025'}</p>
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
                <p className="text-sm font-medium text-green-600">Enrollment Status</p>
                <p className="text-2xl font-bold text-gray-900">{studentData.academicInfo?.enrollmentStatus || 'Active'}</p>
              </div>
            </div>
          </div>

          <div className="p-4 bg-gradient-to-r from-purple-50 to-purple-100 rounded-xl border border-purple-200">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-purple-200 text-purple-600">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-purple-600">Academic Standing</p>
                <p className="text-2xl font-bold text-gray-900">{studentData.academicInfo?.academicStanding || 'Good Standing'}</p>
              </div>
            </div>
          </div>
        </div>
      </Card>

      {/* Enhanced Financial Summary with Fee Balance */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
          <span className="w-2 h-8 bg-gradient-to-b from-purple-500 to-pink-500 rounded-full mr-3"></span>
          Financial Summary
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          <div className="p-4 bg-gradient-to-r from-red-50 to-red-100 rounded-xl border border-red-200">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-red-200 text-red-600">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-red-600">Outstanding Balance</p>
                <p className="text-2xl font-bold text-gray-900">
                  ${(studentData.financialInfo?.balance || 0).toLocaleString()}
                </p>
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
                <p className="text-2xl font-bold text-gray-900">
                  ${(studentData.financialInfo?.totalPaid || 0).toLocaleString()}
                </p>
              </div>
            </div>
          </div>

          <div className="p-4 bg-gradient-to-r from-blue-50 to-blue-100 rounded-xl border border-blue-200">
            <div className="flex items-center">
              <div className="p-3 rounded-full bg-blue-200 text-blue-600">
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                </svg>
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-blue-600">Total Assigned</p>
                <p className="text-2xl font-bold text-gray-900">
                  ${((studentData.financialInfo?.balance || 0) + (studentData.financialInfo?.totalPaid || 0)).toLocaleString()}
                </p>
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

      {/* Fee Structures Section */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
          <span className="w-2 h-8 bg-gradient-to-b from-green-500 to-teal-500 rounded-full mr-3"></span>
          Fee Structures
        </h3>
        
        {/* Semester and Academic Year Selection */}
        <div className="flex flex-wrap gap-4 mb-6">
          <div className="flex-1 min-w-[200px]">
            <label className="block text-sm font-medium text-gray-700 mb-2">Academic Year</label>
            <select
              value={selectedAcademicYear}
              onChange={(e) => setSelectedAcademicYear(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              {availableAcademicYears.map(year => (
                <option key={year} value={year}>{year}</option>
              ))}
            </select>
          </div>
          <div className="flex-1 min-w-[200px]">
            <label className="block text-sm font-medium text-gray-700 mb-2">Semester</label>
            <select
              value={selectedSemester}
              onChange={(e) => setSelectedSemester(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              {availableSemesters.map(semester => (
                <option key={semester} value={semester}>{semester}</option>
              ))}
            </select>
          </div>
        </div>

        {loadingFeeData ? (
          <div className="flex justify-center items-center h-32">
            <LoadingSpinner size="medium" text="Loading fee structures..." />
          </div>
        ) : feeStructures.length > 0 ? (
          <div className="space-y-4">
            {feeStructures.map((structure) => (
              <div key={structure.id} className="p-4 bg-gradient-to-r from-gray-50 to-gray-100/50 rounded-xl border border-gray-200">
                <div className="flex justify-between items-start mb-3">
                  <div>
                    <h4 className="text-lg font-bold text-gray-900">{structure.name}</h4>
                    <p className="text-sm text-gray-600">{structure.description}</p>
                  </div>
                  <div className="text-right">
                    <span className={`px-3 py-1 text-sm font-semibold rounded-full ${
                      structure.isActive 
                        ? 'bg-gradient-to-r from-green-100 to-green-200 text-green-800' 
                        : 'bg-gradient-to-r from-red-100 to-red-200 text-red-800'
                    }`}>
                      {structure.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </div>
                </div>
                
                {structure.feeItems && structure.feeItems.length > 0 && (
                  <div className="space-y-2">
                    <h5 className="font-semibold text-gray-800">Fee Items:</h5>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                      {structure.feeItems.map((item) => (
                        <div key={item.id} className="flex justify-between items-center p-2 bg-white rounded-lg border border-gray-200">
                          <span className="text-sm font-medium text-gray-700">{item.name}</span>
                          <span className="text-sm font-bold text-gray-900">${item.amount.toLocaleString()}</span>
                      </div>
                    ))}
                  </div>
                </div>
                )}
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-8">
            <div className="text-gray-500 mb-2">
              <svg className="w-12 h-12 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <p className="text-gray-600">No fee structures found for the selected semester and academic year.</p>
          </div>
        )}
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