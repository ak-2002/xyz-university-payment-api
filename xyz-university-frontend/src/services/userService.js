import api from './api';

class UserService {
  async getUsers() {
    try {
      console.log('userService.getUsers() called');
      console.log('API Client base URL:', api.defaults.baseURL);
      
      const token = localStorage.getItem('authToken');
      console.log('Auth token exists:', !!token);
      
      const response = await api.get(`/api/v3/user/users?t=${Date.now()}`);
      console.log('API Response:', response);
      console.log('Response data:', response.data);
      
      return response.data;
    } catch (error) {
      console.error('userService.getUsers() error:', error);
      console.error('Error response:', error.response);
      console.error('Error message:', error.message);
      throw error;
    }
  }

  async getUser(id) {
    try {
      const response = await api.get(`/api/v3/user/users/${id}?t=${Date.now()}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching user:', error);
      throw error;
    }
  }

  async createUser(userData) {
    try {
      const response = await api.post(`/api/v3/user/users?t=${Date.now()}`, userData);
      return response.data;
    } catch (error) {
      console.error('Error creating user:', error);
      throw error;
    }
  }

  async assignRole(userId, roleName) {
    try {
      const response = await api.put(`/api/v3/user/users/${userId}/roles?t=${Date.now()}`, {
        roleName: roleName
      });
      return response.data;
    } catch (error) {
      console.error('Error assigning role:', error);
      throw error;
    }
  }

  async toggleUserStatus(userId) {
    try {
      const response = await api.put(`/api/v3/user/users/${userId}/status?t=${Date.now()}`);
      return response.data;
    } catch (error) {
      console.error('Error toggling user status:', error);
      throw error;
    }
  }

  async getRoles() {
    try {
      const response = await api.get(`/api/v3/user/roles?t=${Date.now()}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching roles:', error);
      throw error;
    }
  }
}

export const userService = new UserService(); 