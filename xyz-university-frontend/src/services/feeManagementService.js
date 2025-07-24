import api from './api';

class FeeManagementService {
  // Get all fee structures
  async getAllFeeStructures() {
    try {
      const response = await api.get('/api/v3/feemanagement/structures');
      return response.data;
    } catch (error) {
      console.error('Error fetching fee structures:', error);
      throw error;
    }
  }

  // Get fee structures by academic year and semester
  async getFeeStructuresBySemester(academicYear, semester) {
    try {
      console.log('Making API call to:', `/api/v3/feemanagement/structures/academic-year/${academicYear}/semester/${semester}`);
      const response = await api.get(`/api/v3/feemanagement/structures/academic-year/${academicYear}/semester/${semester}`);
      console.log('API response:', response);
      return response.data;
    } catch (error) {
      console.error('Error fetching fee structures by semester:', error);
      throw error;
    }
  }

  // Get fee structure by ID
  async getFeeStructureById(id) {
    try {
      const response = await api.get(`/api/v3/feemanagement/structures/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching fee structure:', error);
      throw error;
    }
  }

  // Get student fee assignments
  async getStudentFeeAssignments(studentNumber) {
    try {
      const response = await api.get(`/api/v3/feemanagement/students/${studentNumber}/assignments`);
      return response.data;
    } catch (error) {
      console.error('Error fetching student fee assignments:', error);
      throw error;
    }
  }

  // Get student fee balance summary
  async getStudentFeeBalanceSummary(studentNumber) {
    try {
      const response = await api.get(`/api/v3/feemanagement/students/${studentNumber}/balance-summary`);
      return response.data;
    } catch (error) {
      console.error('Error fetching student fee balance summary:', error);
      throw error;
    }
  }

  // Get student fee balances
  async getStudentFeeBalances(studentNumber) {
    try {
      const response = await api.get(`/api/v3/feemanagement/students/${studentNumber}/balances`);
      return response.data;
    } catch (error) {
      console.error('Error fetching student fee balances:', error);
      throw error;
    }
  }

  // Get all fee categories
  async getAllFeeCategories() {
    try {
      const response = await api.get('/api/v3/feemanagement/categories');
      return response.data;
    } catch (error) {
      console.error('Error fetching fee categories:', error);
      throw error;
    }
  }

  // Create fee structure
  async createFeeStructure(feeStructureData) {
    try {
      const response = await api.post('/api/v3/feemanagement/structures', feeStructureData);
      return response.data;
    } catch (error) {
      console.error('Error creating fee structure:', error);
      throw error;
    }
  }

  // Update fee structure
  async updateFeeStructure(id, feeStructureData) {
    try {
      const response = await api.put(`/api/v3/feemanagement/structures/${id}`, feeStructureData);
      return response.data;
    } catch (error) {
      console.error('Error updating fee structure:', error);
      throw error;
    }
  }

  // Reactivate fee structure
  async reactivateFeeStructure(id) {
    try {
      const response = await api.patch(`/api/v3/feemanagement/structures/${id}/reactivate`);
      return response.data;
    } catch (error) {
      console.error('Error reactivating fee structure:', error);
      throw error;
    }
  }

  // Get all fee structures including inactive ones
  async getAllFeeStructuresIncludingInactive() {
    try {
      const response = await api.get('/api/v3/feemanagement/structures/all');
      return response.data;
    } catch (error) {
      console.error('Error fetching all fee structures:', error);
      throw error;
    }
  }

  // Get students with outstanding fees
  async getStudentsWithOutstandingFees() {
    try {
      const response = await api.get('/api/v3/feemanagement/reports/students-outstanding-fees');
      return response.data;
    } catch (error) {
      console.error('Error fetching students with outstanding fees:', error);
      throw error;
    }
  }

  // Get students with overdue fees
  async getStudentsWithOverdueFees() {
    try {
      const response = await api.get('/api/v3/feemanagement/reports/students-overdue-fees');
      return response.data;
    } catch (error) {
      console.error('Error fetching students with overdue fees:', error);
      throw error;
    }
  }

  // Delete fee structure
  async deleteFeeStructure(id) {
    try {
      const response = await api.delete(`/api/v3/feemanagement/structures/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error deleting fee structure:', error);
      throw error;
    }
  }

  // Download fee structures as CSV
  downloadFeeStructuresAsCSV(feeStructures, academicYear, semester) {
    if (!feeStructures || feeStructures.length === 0) {
      alert('No fee structures to download');
      return;
    }

    // Create CSV content
    let csvContent = 'data:text/csv;charset=utf-8,';
    
    // Add header
    csvContent += 'Fee Structure Name,Description,Academic Year,Semester,Status,Total Amount,Fee Category,Fee Amount,Required,Due Date\n';
    
    // Add data rows
    feeStructures.forEach(structure => {
      if (structure.feeStructureItems && structure.feeStructureItems.length > 0) {
        structure.feeStructureItems.forEach((item, index) => {
          const row = [
            index === 0 ? structure.name : '', // Only show structure name in first row
            index === 0 ? structure.description : '',
            index === 0 ? structure.academicYear : '',
            index === 0 ? structure.semester : '',
            index === 0 ? (structure.isActive ? 'Active' : 'Inactive') : '',
            index === 0 ? `$${structure.totalAmount.toLocaleString()}` : '',
            item.feeCategoryName,
            `$${item.amount.toLocaleString()}`,
            item.isRequired ? 'Yes' : 'No',
            item.dueDate ? new Date(item.dueDate).toLocaleDateString() : 'N/A'
          ];
          csvContent += row.map(field => `"${field}"`).join(',') + '\n';
        });
      } else {
        // Structure with no items
        const row = [
          structure.name,
          structure.description,
          structure.academicYear,
          structure.semester,
          structure.isActive ? 'Active' : 'Inactive',
          `$${structure.totalAmount.toLocaleString()}`,
          'No fees assigned',
          '$0.00',
          'N/A',
          'N/A'
        ];
        csvContent += row.map(field => `"${field}"`).join(',') + '\n';
      }
    });

    // Create download link
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `fee_structures_${semester}_${academicYear}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  // Download fee structure summary as PDF (simplified version)
  downloadFeeStructureSummary(feeStructures, academicYear, semester) {
    if (!feeStructures || feeStructures.length === 0) {
      alert('No fee structures to download');
      return;
    }

    // Create a simple HTML table for PDF conversion
    let htmlContent = `
      <html>
        <head>
          <title>Fee Structures - ${semester} ${academicYear}</title>
          <style>
            body { font-family: Arial, sans-serif; margin: 20px; }
            table { border-collapse: collapse; width: 100%; margin-top: 20px; }
            th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
            th { background-color: #f2f2f2; font-weight: bold; }
            .header { text-align: center; margin-bottom: 30px; }
            .structure-header { background-color: #e6f3ff; font-weight: bold; }
            .total-row { background-color: #f9f9f9; font-weight: bold; }
          </style>
        </head>
        <body>
          <div class="header">
            <h1>Fee Structures Report</h1>
            <h2>${semester} ${academicYear}</h2>
            <p>Generated on: ${new Date().toLocaleDateString()}</p>
          </div>
          <table>
            <thead>
              <tr>
                <th>Fee Structure</th>
                <th>Description</th>
                <th>Status</th>
                <th>Fee Category</th>
                <th>Amount</th>
                <th>Required</th>
                <th>Due Date</th>
              </tr>
            </thead>
            <tbody>
    `;

    feeStructures.forEach(structure => {
      if (structure.feeStructureItems && structure.feeStructureItems.length > 0) {
        structure.feeStructureItems.forEach((item, index) => {
          htmlContent += `
            <tr class="${index === 0 ? 'structure-header' : ''}">
              <td>${index === 0 ? structure.name : ''}</td>
              <td>${index === 0 ? structure.description : ''}</td>
              <td>${index === 0 ? (structure.isActive ? 'Active' : 'Inactive') : ''}</td>
              <td>${item.feeCategoryName}</td>
              <td>$${item.amount.toLocaleString()}</td>
              <td>${item.isRequired ? 'Yes' : 'No'}</td>
              <td>${item.dueDate ? new Date(item.dueDate).toLocaleDateString() : 'N/A'}</td>
            </tr>
          `;
        });
        // Add total row
        htmlContent += `
          <tr class="total-row">
            <td colspan="4"><strong>Total for ${structure.name}</strong></td>
            <td><strong>$${structure.totalAmount.toLocaleString()}</strong></td>
            <td colspan="2"></td>
          </tr>
        `;
      } else {
        htmlContent += `
          <tr class="structure-header">
            <td>${structure.name}</td>
            <td>${structure.description}</td>
            <td>${structure.isActive ? 'Active' : 'Inactive'}</td>
            <td>No fees assigned</td>
            <td>$0.00</td>
            <td>N/A</td>
            <td>N/A</td>
          </tr>
        `;
      }
    });

    htmlContent += `
            </tbody>
          </table>
        </body>
      </html>
    `;

    // Create a new window with the HTML content
    const newWindow = window.open('', '_blank');
    newWindow.document.write(htmlContent);
    newWindow.document.close();
    
    // Wait for content to load then print
    newWindow.onload = function() {
      newWindow.print();
    };
  }

  // Get available semesters (helper method)
  getAvailableSemesters() {
    return ['Spring', 'Summer', 'Fall', 'Winter'];
  }

  // Get available academic years (helper method)
  getAvailableAcademicYears() {
    const currentYear = new Date().getFullYear();
    const years = [];
    for (let i = currentYear - 2; i <= currentYear + 2; i++) {
      years.push(i.toString());
    }
    return years;
  }

  // Apply additional fees to students
  async applyAdditionalFees() {
    try {
      const response = await api.post('/api/v3/feemanagement/apply-additional-fees');
      return response.data;
    } catch (error) {
      console.error('Error applying additional fees:', error);
      throw error;
    }
  }

  // Migrate old student balances to new system
  async migrateOldBalances() {
    try {
      console.log('Starting migration of old student balances...');
      const response = await api.post('/api/v3/feemanagement/utility/migrate-old-balances');
      console.log('Migration response:', response.data);
      return response.data;
    } catch (error) {
      console.error('Error migrating old balances:', error);
      throw error;
    }
  }

  // Debug method to check balance table status
  async debugBalanceTables() {
    try {
      console.log('Checking balance table status...');
      const response = await api.get('/api/v3/feemanagement/utility/debug-balances');
      console.log('Debug response:', response.data);
      return response.data;
    } catch (error) {
      console.error('Error checking balance tables:', error);
      throw error;
    }
  }

  // Test database connectivity
  async testDatabase() {
    try {
      console.log('Testing database connectivity...');
      const response = await api.get('/api/v3/feemanagement/utility/test-database');
      console.log('Database test response:', response.data);
      return response.data;
    } catch (error) {
      console.error('Error testing database:', error);
      throw error;
    }
  }

  // Reconcile StudentFeeBalance records with actual payments
  async reconcileFeeBalances() {
    try {
      const response = await api.post('/api/v3/feemanagement/utility/reconcile-fee-balances');
      return response.data;
    } catch (error) {
      console.error('Error reconciling fee balances:', error);
      throw error;
    }
  }

  // Assign Summer 2025 fee structure to all students
  async assignFeeStructureToAllStudents() {
    try {
      const response = await api.post('/api/v3/feemanagement/utility/assign-fee-structure-to-all');
      return response.data;
    } catch (error) {
      console.error('Error assigning fee structure to all students:', error);
      throw error;
    }
  }

  // Flexible assignment of fee structure to students
  async assignFeeStructureFlexible(options) {
    try {
      const response = await api.post('/api/v3/feemanagement/utility/assign-fee-structure', options);
      return response.data;
    } catch (error) {
      console.error('Error in flexible fee structure assignment:', error);
      throw error;
    }
  }

  // Assign fee structure to all students with outstanding balance handling
  async assignFeeStructureToAll(feeStructureId) {
    try {
      const response = await api.post('/api/v3/feemanagement/utility/assign-fee-structure-to-all', { feeStructureId });
      return response.data;
    } catch (error) {
      console.error('Error in assigning fee structure to all students:', error);
      throw error;
    }
  }
}

export const feeManagementService = new FeeManagementService();
export default feeManagementService; 