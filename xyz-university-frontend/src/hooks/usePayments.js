import { useState, useCallback } from 'react';
import { paymentService } from '../services/paymentService';

export const usePayments = () => {
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  });

  // Get all payments
  const getPayments = useCallback(async (page = 1, pageSize = 10, searchTerm = '') => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await paymentService.getPayments(page, pageSize, searchTerm);
      
      setPayments(response.data || []);
      setPagination({
        page: response.page || page,
        pageSize: response.pageSize || pageSize,
        totalCount: response.totalCount || 0,
        totalPages: response.totalPages || 0,
      });
      
      return response;
    } catch (error) {
      setError(error.message || 'Failed to fetch payments');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Get payment by ID
  const getPaymentById = useCallback(async (id) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await paymentService.getPaymentById(id);
      return response;
    } catch (error) {
      setError(error.message || 'Failed to fetch payment');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Create payment
  const createPayment = useCallback(async (paymentData) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await paymentService.createPayment(paymentData);
      
      // Refresh the payments list
      await getPayments(pagination.page, pagination.pageSize);
      
      return response;
    } catch (error) {
      setError(error.message || 'Failed to create payment');
      throw error;
    } finally {
      setLoading(false);
    }
  }, [getPayments, pagination.page, pagination.pageSize]);

  // Get payments by student
  const getPaymentsByStudent = useCallback(async (studentNumber, page = 1, pageSize = 10) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await paymentService.getPaymentsByStudent(studentNumber, page, pageSize);
      
      setPayments(response.data || []);
      setPagination({
        page: response.page || page,
        pageSize: response.pageSize || pageSize,
        totalCount: response.totalCount || 0,
        totalPages: response.totalPages || 0,
      });
      
      return response;
    } catch (error) {
      setError(error.message || 'Failed to fetch student payments');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Get payment summary by student
  const getPaymentSummaryByStudent = useCallback(async (studentNumber) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await paymentService.getPaymentSummaryByStudent(studentNumber);
      return response;
    } catch (error) {
      setError(error.message || 'Failed to fetch payment summary');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Get payment statistics
  const getPaymentStatistics = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await paymentService.getPaymentStatistics();
      return response;
    } catch (error) {
      setError(error.message || 'Failed to fetch payment statistics');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Get payments by date range
  const getPaymentsByDateRange = useCallback(async (startDate, endDate, page = 1, pageSize = 10) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await paymentService.getPaymentsByDateRange(startDate, endDate, page, pageSize);
      
      setPayments(response.data || []);
      setPagination({
        page: response.page || page,
        pageSize: response.pageSize || pageSize,
        totalCount: response.totalCount || 0,
        totalPages: response.totalPages || 0,
      });
      
      return response;
    } catch (error) {
      setError(error.message || 'Failed to fetch payments by date range');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Process payment notification
  const processPaymentNotification = useCallback(async (notificationData) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await paymentService.processPaymentNotification(notificationData);
      
      // Refresh the payments list
      await getPayments(pagination.page, pagination.pageSize);
      
      return response;
    } catch (error) {
      setError(error.message || 'Failed to process payment notification');
      throw error;
    } finally {
      setLoading(false);
    }
  }, [getPayments, pagination.page, pagination.pageSize]);

  // Validate payment
  const validatePayment = useCallback(async (paymentData) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await paymentService.validatePayment(paymentData);
      return response;
    } catch (error) {
      setError(error.message || 'Failed to validate payment');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Clear error
  const clearError = useCallback(() => {
    setError(null);
  }, []);

  return {
    payments,
    loading,
    error,
    pagination,
    getPayments,
    getPaymentById,
    createPayment,
    getPaymentsByStudent,
    getPaymentSummaryByStudent,
    getPaymentStatistics,
    getPaymentsByDateRange,
    processPaymentNotification,
    validatePayment,
    clearError,
  };
}; 