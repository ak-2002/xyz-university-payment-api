import React, { useState, useEffect } from 'react';
import { useAuth } from '../hooks/useAuth';
import { userService } from '../services/userService';
import { studentService } from '../services/studentService';
import Card from '../components/common/Card';
import LoadingSpinner from '../components/common/LoadingSpinner';
import NotificationModal from '../components/common/NotificationModal';

const RoleManagement = () => {
  const { user } = useAuth();
  const [users, setUsers] = useState([]);
  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [notification, setNotification] = useState({
    isOpen: false,
    type: 'info',
    title: '',
    message: '',
    details: ''
  });
  const [selectedUser, setSelectedUser] = useState(null);
  const [showRoleModal, setShowRoleModal] = useState(false);
  const [showCreateUserModal, setShowCreateUserModal] = useState(false);
  const [availableRoles, setAvailableRoles] = useState([
    { id: 1, name: 'Admin', description: 'Full system access and management' },
    { id: 2, name: 'Manager', description: 'Department management and oversight' },
    { id: 3, name: 'Staff', description: 'Basic operational access' },
    { id: 4, name: 'Student', description: 'Student portal access only' }
  ]);
  const [newUserForm, setNewUserForm] = useState({
    username: '',
    email: '',
    fullName: '',
    password: '',
    defaultRole: 'Student',
    selectedStudentId: null
  });

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      setLoading(true);
      console.log('Loading users...');
      const users = await userService.getUsers();
      console.log('Users loaded:', users);
      setUsers(users);
    } catch (err) {
      console.error('Error loading users:', err);
      setError('Failed to load users');
    } finally {
      setLoading(false);
    }
  };

  const loadStudents = async () => {
    try {
      console.log('Loading students for user creation...');
      const students = await studentService.getStudents(1, 100); // Get all students
      console.log('Students loaded:', students);
      setStudents(students);
    } catch (err) {
      console.error('Error loading students:', err);
      showNotification('error', 'Error Loading Students', 'Failed to load students for user creation.');
    }
  };

  const handleCreateUserModalOpen = () => {
    setShowCreateUserModal(true);
    loadStudents(); // Load students when modal opens
  };

  const handleStudentSelection = (studentId) => {
    const selectedStudent = students.find(s => s.id === parseInt(studentId));
    if (selectedStudent) {
      setNewUserForm({
        ...newUserForm,
        selectedStudentId: studentId,
        username: selectedStudent.studentNumber || '', // Use student number as username
        email: selectedStudent.email || `${selectedStudent.studentNumber}@xyz.edu` || '',
        fullName: selectedStudent.fullName || selectedStudent.name || '',
        password: 'Password123!' // Default password
      });
    }
  };

  const handleRoleChange = (role) => {
    setNewUserForm({
      ...newUserForm,
      defaultRole: role,
      selectedStudentId: role === 'Student' ? newUserForm.selectedStudentId : null
    });
  };

  const handleAssignRole = async (userId, newRole) => {
    try {
      await userService.assignRole(userId, newRole);
      
      // Update the local state to reflect the change
      setUsers(prevUsers => 
        prevUsers.map(u => 
          u.id === userId 
            ? { ...u, currentRoles: [newRole] }
            : u
        )
      );
      
      setShowRoleModal(false);
      setSelectedUser(null);
      showNotification('success', 'Role Assigned', `Role has been successfully assigned to the user.`);
    } catch (err) {
      console.error(err);
      const errorMessage = err.response?.data?.message || 'Failed to assign role';
      showNotification('error', 'Role Assignment Failed', errorMessage);
    }
  };

  const handleToggleUserStatus = async (userId, currentStatus) => {
    try {
      await userService.toggleUserStatus(userId);
      
      // Update the local state to reflect the change
      setUsers(prevUsers => 
        prevUsers.map(u => 
          u.id === userId 
            ? { ...u, isActive: !currentStatus }
            : u
        )
      );
      
      showNotification('success', 'Status Updated', `User status has been ${!currentStatus ? 'activated' : 'deactivated'}.`);
    } catch (err) {
      console.error(err);
      const errorMessage = err.response?.data?.message || 'Failed to update user status';
      showNotification('error', 'Status Update Failed', errorMessage);
    }
  };

  const handleCreateUser = async (e) => {
    e.preventDefault();
    
    // Validate that a student is selected when creating a student user
    if (newUserForm.defaultRole === 'Student' && !newUserForm.selectedStudentId) {
      showNotification('error', 'Student Selection Required', 'Please select a student from the dropdown when creating a student user.');
      return;
    }
    
    try {
      await userService.createUser(newUserForm);
      setShowCreateUserModal(false);
      setNewUserForm({ username: '', email: '', fullName: '', password: '', defaultRole: 'Student', selectedStudentId: null });
      loadUsers(); // Reload the users list
      showNotification('success', 'User Created', 'New user has been created successfully.');
    } catch (err) {
      console.error(err);
      const errorMessage = err.response?.data?.message || 'Failed to create user';
      showNotification('error', 'User Creation Failed', errorMessage);
    }
  };

  const showNotification = (type, title, message, details = '') => {
    setNotification({
      isOpen: true,
      type,
      title,
      message,
      details
    });
  };

  const closeNotification = () => {
    setNotification(prev => ({ ...prev, isOpen: false }));
  };

  const openRoleModal = (user) => {
    setSelectedUser(user);
    setShowRoleModal(true);
  };

  const getRoleBadgeColor = (role) => {
    switch (role.toLowerCase()) {
      case 'admin':
        return 'bg-red-100 text-red-800';
      case 'manager':
        return 'bg-blue-100 text-blue-800';
      case 'staff':
        return 'bg-green-100 text-green-800';
      case 'student':
        return 'bg-purple-100 text-purple-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusBadgeColor = (isActive) => {
    return isActive 
      ? 'bg-green-100 text-green-800' 
      : 'bg-red-100 text-red-800';
  };

  if (loading) {
    return <LoadingSpinner size="large" text="Loading users..." />;
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50/30 to-purple-50/30 p-6 space-y-8">
      {/* Header */}
      <Card variant="elevated" className="p-8">
        <div className="text-center">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent mb-3">
            Role Management
          </h1>
          <p className="text-lg text-gray-600 mb-6">
            Manage user roles and permissions across the system
          </p>
        </div>
      </Card>

      {/* Statistics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-blue-100 to-blue-200 text-blue-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Total Users</p>
              <p className="text-3xl font-bold text-gray-900">{users.length}</p>
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
              <p className="text-sm font-medium text-gray-600 mb-1">Active Users</p>
              <p className="text-3xl font-bold text-gray-900">{users.filter(u => u.isActive).length}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-purple-100 to-purple-200 text-purple-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Students</p>
              <p className="text-3xl font-bold text-gray-900">{users.filter(u => u.currentRoles.includes('Student')).length}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-orange-100 to-orange-200 text-orange-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Staff</p>
              <p className="text-3xl font-bold text-gray-900">{users.filter(u => u.currentRoles.includes('Manager') || u.currentRoles.includes('Staff')).length}</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Users Table */}
      <Card variant="elevated" className="p-6">
        <div className="flex justify-between items-center mb-6">
          <h3 className="text-xl font-bold text-gray-900 flex items-center">
            <span className="w-2 h-8 bg-gradient-to-b from-purple-500 to-pink-500 rounded-full mr-3"></span>
            User Management
          </h3>
          <button
            onClick={handleCreateUserModalOpen}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium"
          >
            Create New User
          </button>
        </div>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gradient-to-r from-gray-50 to-gray-100/50">
              <tr>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  User
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Current Role
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Created
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-100">
              {users.map((userItem) => (
                <tr key={userItem.id} className="hover:bg-gray-50/50 transition-colors duration-200">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <div className="h-10 w-10 rounded-full bg-blue-600 flex items-center justify-center">
                        <span className="text-white font-medium">
                          {userItem.fullName?.charAt(0) || 'U'}
                        </span>
                      </div>
                      <div className="ml-4">
                        <div className="text-sm font-medium text-gray-900">{userItem.fullName}</div>
                        <div className="text-sm text-gray-500">{userItem.email}</div>
                        <div className="text-xs text-gray-400">@{userItem.username}</div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {userItem.currentRoles.map((role, index) => (
                      <span
                        key={index}
                        className={`inline-flex px-3 py-1 text-xs font-semibold rounded-full ${getRoleBadgeColor(role)}`}
                      >
                        {role}
                      </span>
                    ))}
                    {userItem.isStudent && (
                      <span className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-purple-100 text-purple-800 ml-1">
                        Student
                      </span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`inline-flex px-3 py-1 text-xs font-semibold rounded-full ${getStatusBadgeColor(userItem.isActive)}`}>
                      {userItem.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {new Date(userItem.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <div className="flex space-x-2">
                      <button
                        onClick={() => openRoleModal(userItem)}
                        className="text-blue-600 hover:text-blue-900 text-sm"
                      >
                        Change Role
                      </button>
                      <button
                        onClick={() => handleToggleUserStatus(userItem.id, userItem.isActive)}
                        className={`text-sm ${
                          userItem.isActive 
                            ? 'text-red-600 hover:text-red-900' 
                            : 'text-green-600 hover:text-green-900'
                        }`}
                      >
                        {userItem.isActive ? 'Deactivate' : 'Activate'}
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Role Assignment Modal */}
      {showRoleModal && selectedUser && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Assign Role</h3>
              <div className="mb-4">
                <p className="text-sm text-gray-600 mb-2">
                  Current user: <span className="font-medium">{selectedUser.fullName}</span>
                </p>
                <p className="text-sm text-gray-600">
                  Current role: <span className="font-medium">{selectedUser.currentRoles.join(', ')}</span>
                </p>
              </div>
              <div className="space-y-3">
                {availableRoles.map((role) => (
                  <button
                    key={role.id}
                    onClick={() => handleAssignRole(selectedUser.id, role.name)}
                    className={`w-full p-3 text-left rounded-lg border transition-colors ${
                      selectedUser.currentRoles.includes(role.name)
                        ? 'bg-blue-50 border-blue-200 text-blue-900'
                        : 'bg-white border-gray-200 text-gray-700 hover:bg-gray-50'
                    }`}
                  >
                    <div className="font-medium">{role.name}</div>
                    <div className="text-sm text-gray-500">{role.description}</div>
                  </button>
                ))}
              </div>
              <div className="flex justify-end mt-6">
                <button
                  onClick={() => {
                    setShowRoleModal(false);
                    setSelectedUser(null);
                  }}
                  className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 border border-gray-300 rounded-md hover:bg-gray-200"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Create User Modal */}
      {showCreateUserModal && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <h3 className="text-lg font-medium text-gray-900 mb-4">Create New User</h3>
              <form onSubmit={handleCreateUser} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700">Username</label>
                  <input
                    type="text"
                    value={newUserForm.username}
                    onChange={(e) => setNewUserForm({...newUserForm, username: e.target.value})}
                    className={`mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 ${
                      newUserForm.defaultRole === 'Student' && newUserForm.selectedStudentId ? 'bg-gray-100' : ''
                    }`}
                    required
                    placeholder="Enter username"
                    readOnly={newUserForm.defaultRole === 'Student' && newUserForm.selectedStudentId}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Email</label>
                  <input
                    type="email"
                    value={newUserForm.email}
                    onChange={(e) => setNewUserForm({...newUserForm, email: e.target.value})}
                    className={`mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 ${
                      newUserForm.defaultRole === 'Student' && newUserForm.selectedStudentId ? 'bg-gray-100' : ''
                    }`}
                    required
                    placeholder="Enter email"
                    readOnly={newUserForm.defaultRole === 'Student' && newUserForm.selectedStudentId}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Full Name</label>
                  <input
                    type="text"
                    value={newUserForm.fullName}
                    onChange={(e) => setNewUserForm({...newUserForm, fullName: e.target.value})}
                    className={`mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 ${
                      newUserForm.defaultRole === 'Student' && newUserForm.selectedStudentId ? 'bg-gray-100' : ''
                    }`}
                    required
                    placeholder="Enter full name"
                    readOnly={newUserForm.defaultRole === 'Student' && newUserForm.selectedStudentId}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Password</label>
                  <input
                    type="password"
                    value={newUserForm.password}
                    onChange={(e) => setNewUserForm({...newUserForm, password: e.target.value})}
                    className={`mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 ${
                      newUserForm.defaultRole === 'Student' && newUserForm.selectedStudentId ? 'bg-gray-100' : ''
                    }`}
                    required
                    placeholder="Enter password"
                    readOnly={newUserForm.defaultRole === 'Student' && newUserForm.selectedStudentId}
                  />
                  {newUserForm.defaultRole === 'Student' && newUserForm.selectedStudentId && (
                    <p className="mt-1 text-xs text-gray-500">Default password will be used</p>
                  )}
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Default Role</label>
                  <select
                    value={newUserForm.defaultRole}
                    onChange={(e) => handleRoleChange(e.target.value)}
                    className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    required
                  >
                    <option value="Student">Student</option>
                    <option value="Staff">Staff</option>
                    <option value="Manager">Manager</option>
                    <option value="Admin">Admin</option>
                  </select>
                </div>
                {newUserForm.defaultRole === 'Student' && (
                  <div>
                    <label className="block text-sm font-medium text-gray-700">Select Student</label>
                    <select
                      value={newUserForm.selectedStudentId}
                      onChange={(e) => handleStudentSelection(e.target.value)}
                      className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                      required
                    >
                      <option value="">Select a student</option>
                      {students
                        .filter(student => !users.some(user => user.username === student.studentNumber))
                        .map(student => (
                          <option key={student.id} value={student.id}>
                            {student.studentNumber} - {student.fullName || student.name}
                          </option>
                        ))}
                    </select>
                    {students.filter(student => !users.some(user => user.username === student.studentNumber)).length === 0 && (
                      <p className="mt-1 text-xs text-orange-600">
                        All students already have user accounts
                      </p>
                    )}
                    {newUserForm.selectedStudentId && (
                      <div className="mt-2 p-2 bg-green-50 border border-green-200 rounded-md">
                        <p className="text-xs text-green-700">
                          ✓ Student selected - Form fields will be auto-filled with student data
                        </p>
                      </div>
                    )}
                  </div>
                )}
                <div className="flex justify-end space-x-3 pt-4">
                  <button
                    type="button"
                    onClick={() => {
                      setShowCreateUserModal(false);
                      setNewUserForm({ username: '', email: '', fullName: '', password: '', defaultRole: 'Student', selectedStudentId: null });
                    }}
                    className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 border border-gray-300 rounded-md hover:bg-gray-200"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700"
                  >
                    Create User
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Notification Modal */}
      <NotificationModal
        isOpen={notification.isOpen}
        onClose={closeNotification}
        type={notification.type}
        title={notification.title}
        message={notification.message}
        details={notification.details}
      />
    </div>
  );
};

export default RoleManagement; 