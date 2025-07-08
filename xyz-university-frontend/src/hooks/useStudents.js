import { useState, useCallback } from 'react';
import { studentService } from '../services/studentService';

export const useStudents = () => {
  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  });

  // Get all students
  const getStudents = useCallback(async (page = 1, pageSize = 10, searchTerm = '') => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await studentService.getStudents(page, pageSize, searchTerm);
      
      setStudents(response.data || []);
      setPagination({
        page: response.page || page,
        pageSize: response.pageSize || pageSize,
        totalCount: response.totalCount || 0,
        totalPages: response.totalPages || 0,
      });
      
      return response;
    } catch (error) {
      setError(error.message || 'Failed to fetch students');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Get student by ID
  const getStudentById = useCallback(async (id) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await studentService.getStudentById(id);
      return response;
    } catch (error) {
      setError(error.message || 'Failed to fetch student');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Create student
  const createStudent = useCallback(async (studentData) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await studentService.createStudent(studentData);
      
      // Refresh the students list
      await getStudents(pagination.page, pagination.pageSize);
      
      return response;
    } catch (error) {
      setError(error.message || 'Failed to create student');
      throw error;
    } finally {
      setLoading(false);
    }
  }, [getStudents, pagination.page, pagination.pageSize]);

  // Update student
  const updateStudent = useCallback(async (id, studentData) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await studentService.updateStudent(id, studentData);
      
      // Update the student in the list
      setStudents(prevStudents => 
        prevStudents.map(student => 
          student.id === id ? { ...student, ...response } : student
        )
      );
      
      return response;
    } catch (error) {
      setError(error.message || 'Failed to update student');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Delete student
  const deleteStudent = useCallback(async (id) => {
    try {
      setLoading(true);
      setError(null);
      
      await studentService.deleteStudent(id);
      
      // Remove the student from the list
      setStudents(prevStudents => prevStudents.filter(student => student.id !== id));
      
      // Refresh the list if current page is empty
      if (students.length === 1 && pagination.page > 1) {
        await getStudents(pagination.page - 1, pagination.pageSize);
      }
    } catch (error) {
      setError(error.message || 'Failed to delete student');
      throw error;
    } finally {
      setLoading(false);
    }
  }, [students.length, pagination.page, pagination.pageSize, getStudents]);

  // Search students
  const searchStudents = useCallback(async (searchTerm, page = 1, pageSize = 10) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await studentService.searchStudents(searchTerm, page, pageSize);
      
      setStudents(response.data || []);
      setPagination({
        page: response.page || page,
        pageSize: response.pageSize || pageSize,
        totalCount: response.totalCount || 0,
        totalPages: response.totalPages || 0,
      });
      
      return response;
    } catch (error) {
      setError(error.message || 'Failed to search students');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Get active students
  const getActiveStudents = useCallback(async (page = 1, pageSize = 10) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await studentService.getActiveStudents(page, pageSize);
      
      setStudents(response.data || []);
      setPagination({
        page: response.page || page,
        pageSize: response.pageSize || pageSize,
        totalCount: response.totalCount || 0,
        totalPages: response.totalPages || 0,
      });
      
      return response;
    } catch (error) {
      setError(error.message || 'Failed to fetch active students');
      throw error;
    } finally {
      setLoading(false);
    }
  }, []);

  // Update student status
  const updateStudentStatus = useCallback(async (id, isActive) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await studentService.updateStudentStatus(id, isActive);
      
      // Update the student status in the list
      setStudents(prevStudents => 
        prevStudents.map(student => 
          student.id === id ? { ...student, isActive } : student
        )
      );
      
      return response;
    } catch (error) {
      setError(error.message || 'Failed to update student status');
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
    students,
    loading,
    error,
    pagination,
    getStudents,
    getStudentById,
    createStudent,
    updateStudent,
    deleteStudent,
    searchStudents,
    getActiveStudents,
    updateStudentStatus,
    clearError,
  };
}; 