import api from './api';

export const paymentService = {
  // Get all payments with pagination
  async getPayments(page = 1, pageSize = 10, searchTerm = '') {
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      });
      
      if (searchTerm) {
        params.append('searchTerm', searchTerm);
      }

      console.log('Fetching payments with params:', params.toString());
      const response = await api.get(`/api/v3/Payment?${params}`);
      console.log('Payments API response:', response.data);
      
      // Handle the wrapped API response structure for payments
      if (response.data && response.data.success && response.data.data) {
        // Payments API returns: ApiResponse<PagedResultDto<PaymentDto>>
        // The actual data is in response.data.data.items
        const result = response.data.data.items || [];
        console.log('Processed payments data:', result);
        return result;
      }
      console.log('Returning raw response data:', response.data);
      return response.data || [];
    } catch (error) {
      console.error('Error fetching payments:', error);
      console.error('Error response:', error.response?.data);
      throw error;
    }
  },

  // Get payment by ID
  async getPaymentById(id) {
    try {
      const response = await api.get(`/api/v3/Payment/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching payment:', error);
      throw error;
    }
  },

  // Create new payment notification
  async createPayment(paymentData) {
    try {
      const response = await api.post('/api/v3/Payment', paymentData);
      return response.data;
    } catch (error) {
      console.error('Error creating payment:', error);
      throw error;
    }
  },

  // Get payments by student number
  async getPaymentsByStudent(studentNumber, page = 1, pageSize = 10) {
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
      });

      console.log('Fetching payments for student:', studentNumber);
      const response = await api.get(`/api/v3/Payment/student/${studentNumber}?${params}`);
      console.log('Student payments API response:', response.data);
      
      // Handle the wrapped API response structure
      if (response.data && response.data.success && response.data.data) {
        // The actual data is in response.data.data.items
        const result = response.data.data.items || [];
        console.log('Processed student payments data:', result);
        return {
          data: result,
          page: response.data.data.pageNumber || page,
          pageSize: response.data.data.pageSize || pageSize,
          totalCount: response.data.data.totalCount || 0,
          totalPages: response.data.data.totalPages || 0,
        };
      }
      
      console.log('Returning raw response data:', response.data);
      return response.data || { data: [] };
    } catch (error) {
      console.error('Error fetching payments by student:', error);
      console.error('Error response:', error.response?.data);
      throw error;
    }
  },

  // Get payment summary by student
  async getPaymentSummaryByStudent(studentNumber) {
    try {
      console.log('Fetching payment summary for student:', studentNumber);
      const response = await api.get(`/api/v3/Payment/student/${studentNumber}/summary`);
      console.log('Payment summary API response:', response.data);
      
      // Handle the wrapped API response structure
      if (response.data && response.data.success && response.data.data) {
        const result = response.data.data;
        console.log('Processed payment summary data:', result);
        return result;
      }
      
      console.log('Returning raw response data:', response.data);
      return response.data || {};
    } catch (error) {
      console.error('Error fetching payment summary:', error);
      console.error('Error response:', error.response?.data);
      throw error;
    }
  },

  // Get payment statistics
  async getPaymentStatistics() {
    try {
      const response = await api.get('/api/v3/Payment/statistics');
      return response.data;
    } catch (error) {
      console.error('Error fetching payment statistics:', error);
      throw error;
    }
  },

  // Get payments by date range
  async getPaymentsByDateRange(startDate, endDate, page = 1, pageSize = 10) {
    try {
      const params = new URLSearchParams({
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString(),
        page: page.toString(),
        pageSize: pageSize.toString(),
      });

      const response = await api.get(`/api/v3/Payment/date-range?${params}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching payments by date range:', error);
      throw error;
    }
  },

  // Validate payment data
  async validatePayment(paymentData) {
    try {
      const response = await api.post('/api/v3/Payment/validate', paymentData);
      return response.data;
    } catch (error) {
      console.error('Error validating payment:', error);
      throw error;
    }
  },

  // Process payment notification (from Family Bank)
  async processPaymentNotification(notificationData) {
    try {
      const response = await api.post('/api/v3/Payment/notify', notificationData);
      return response.data;
    } catch (error) {
      console.error('Error processing payment notification:', error);
      throw error;
    }
  },
}; 