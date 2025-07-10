import api from './api';

export const dashboardService = {
  // Get admin dashboard statistics
  async getAdminStats() {
    try {
      const response = await api.get('/api/v3/Dashboard/admin-stats');
      return response.data;
    } catch (error) {
      console.error('Error fetching admin stats:', error);
      throw error;
    }
  },

  // Get manager dashboard statistics
  async getManagerStats() {
    try {
      const response = await api.get('/api/v3/Dashboard/manager-stats');
      return response.data;
    } catch (error) {
      console.error('Error fetching manager stats:', error);
      throw error;
    }
  },

  // Get staff dashboard statistics
  async getStaffStats() {
    try {
      const response = await api.get('/api/v3/Dashboard/staff-stats');
      return response.data;
    } catch (error) {
      console.error('Error fetching staff stats:', error);
      throw error;
    }
  },

  // Get student dashboard statistics
  async getStudentStats() {
    try {
      console.log('DashboardService: Calling student-stats endpoint...');
      const response = await api.get('/api/v3/Dashboard/student-stats');
      console.log('DashboardService: Raw response:', response);
      console.log('DashboardService: Response data:', response.data);
      return response.data;
    } catch (error) {
      console.error('DashboardService: Error fetching student stats:', error);
      console.error('DashboardService: Error response:', error.response?.data);
      console.error('DashboardService: Error status:', error.response?.status);
      throw error;
    }
  },

  // Get role-based dashboard statistics
  async getRoleBasedStats(userRole) {
    try {
      switch (userRole) {
        case 'admin':
          return await this.getAdminStats();
        case 'manager':
          return await this.getManagerStats();
        case 'staff':
          return await this.getStaffStats();
        case 'student':
        default:
          return await this.getStudentStats();
      }
    } catch (error) {
      console.error('Error fetching role-based stats:', error);
      throw error;
    }
  }
}; 