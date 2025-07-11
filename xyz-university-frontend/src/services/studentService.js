import api from './api';

export const studentService = {
  // Get all students with pagination
  async getStudents(page = 1, pageSize = 10, searchTerm = '') {
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      });
      
      if (searchTerm) {
        params.append('searchTerm', searchTerm);
      }

      console.log('Fetching students with params:', params.toString());
      const response = await api.get(`/api/v3/StudentControllerV?${params}`);
      console.log('Students API response:', response.data);
      
      // Handle the wrapped API response structure for students
      if (response.data && response.data.success && response.data.data) {
        // Students API returns: ApiResponse<PaginatedResponseDto<StudentDtoV3>>
        // The actual data is in response.data.data.data
        const result = response.data.data.data || [];
        console.log('Processed students data:', result);
        return result;
      }
      console.log('Returning raw response data:', response.data);
      return response.data || [];
    } catch (error) {
      console.error('Error fetching students:', error);
      console.error('Error response:', error.response?.data);
      throw error;
    }
  },

  // Get student by ID
  async getStudentById(id) {
    try {
      const response = await api.get(`/api/v3/StudentControllerV/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching student:', error);
      throw error;
    }
  },

  // Get student by student number
  async getStudentByNumber(studentNumber) {
    try {
      const response = await api.get(`/api/v3/StudentControllerV/number/${studentNumber}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching student by number:', error);
      throw error;
    }
  },

  // Create new student
  async createStudent(studentData) {
    try {
      const response = await api.post('/api/v3/StudentControllerV', studentData);
      return response.data;
    } catch (error) {
      console.error('Error creating student:', error);
      throw error;
    }
  },

  // Update student
  async updateStudent(id, studentData) {
    try {
      const response = await api.put(`/api/v3/StudentControllerV/${id}`, studentData);
      return response.data;
    } catch (error) {
      console.error('Error updating student:', error);
      throw error;
    }
  },

  // Delete student
  async deleteStudent(id, permanent = false) {
    try {
      const response = await api.delete(`/api/v3/StudentControllerV/${id}?permanent=${permanent}`);
      return response.data;
    } catch (error) {
      console.error('Error deleting student:', error);
      throw error;
    }
  },

  // Get active students
  async getActiveStudents(page = 1, pageSize = 10) {
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      });

      const response = await api.get(`/api/v3/StudentControllerV/active?${params}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching active students:', error);
      throw error;
    }
  },

  // Get students by program
  async getStudentsByProgram(program, page = 1, pageSize = 10) {
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      });

      const response = await api.get(`/api/v3/StudentControllerV/program/${program}?${params}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching students by program:', error);
      throw error;
    }
  },

  // Search students
  async searchStudents(searchTerm, page = 1, pageSize = 10) {
    try {
      const params = new URLSearchParams({
        searchTerm,
        page: page.toString(),
        pageSize: pageSize.toString(),
      });

      const response = await api.get(`/api/v3/StudentControllerV/search?${params}`);
      return response.data;
    } catch (error) {
      console.error('Error searching students:', error);
      throw error;
    }
  },

  // Update student status
  async updateStudentStatus(id, isActive) {
    try {
      const response = await api.put(`/api/v3/StudentControllerV/${id}/status`, { isActive });
      return response.data;
    } catch (error) {
      console.error('Error updating student status:', error);
      throw error;
    }
  },

  // Validate student data
  async validateStudent(studentData) {
    try {
      const response = await api.post('/api/v3/StudentControllerV/validate', studentData);
      return response.data;
    } catch (error) {
      console.error('Error validating student:', error);
      throw error;
    }
  },
}; 