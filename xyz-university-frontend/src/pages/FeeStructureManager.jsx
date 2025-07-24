import React, { useState, useEffect } from 'react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import Alert from '../components/common/ErrorMessage';
import Spinner from '../components/common/LoadingSpinner';
import Modal from '../components/common/Modal';
import { feeManagementService } from '../services/feeManagementService';

const FeeStructureManager = () => {
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [feeStructures, setFeeStructures] = useState([]);
    const [allFeeStructures, setAllFeeStructures] = useState([]);
    const [feeCategories, setFeeCategories] = useState([]);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [showEditModal, setShowEditModal] = useState(false);
    const [showInactiveModal, setShowInactiveModal] = useState(false);
    const [selectedStructure, setSelectedStructure] = useState(null);
    const [formData, setFormData] = useState({
        name: '',
        description: '',
        academicYear: '',
        semester: '',
        feeStructureItems: []
    });

    const availableSemesters = feeManagementService.getAvailableSemesters();
    const availableAcademicYears = feeManagementService.getAvailableAcademicYears();

    useEffect(() => {
        loadData();
    }, []);

    const loadData = async () => {
        try {
            setLoading(true);
            setError(null);

            const [structures, categories] = await Promise.all([
                feeManagementService.getAllFeeStructures(),
                feeManagementService.getAllFeeCategories()
            ]);

            setFeeStructures(structures);
            setFeeCategories(categories);
        } catch (err) {
            setError('Failed to load fee structures and categories');
            console.error('Error loading data:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleCreateStructure = () => {
        setFormData({
            name: '',
            description: '',
            academicYear: new Date().getFullYear().toString(),
            semester: 'Fall',
            isActive: true,
            feeStructureItems: []
        });
        setShowCreateModal(true);
    };

    const handleEditStructure = (structure) => {
        setSelectedStructure(structure);
        setFormData({
            name: structure.name,
            description: structure.description,
            academicYear: structure.academicYear,
            semester: structure.semester,
            isActive: structure.isActive,
            feeStructureItems: [...structure.feeStructureItems]
        });
        setShowEditModal(true);
    };

    const handleAddFeeItem = () => {
        const newItem = {
            feeCategoryId: '',
            amount: 0,
            isRequired: true,
            description: '',
            dueDate: ''
        };
        setFormData(prev => ({
            ...prev,
            feeStructureItems: [...prev.feeStructureItems, newItem]
        }));
    };

    const handleRemoveFeeItem = (index) => {
        setFormData(prev => ({
            ...prev,
            feeStructureItems: prev.feeStructureItems.filter((_, i) => i !== index)
        }));
    };

    const handleFeeItemChange = (index, field, value) => {
        setFormData(prev => ({
            ...prev,
            feeStructureItems: prev.feeStructureItems.map((item, i) => 
                i === index ? { ...item, [field]: value } : item
            )
        }));
    };

    const handleSubmit = async () => {
        try {
            setLoading(true);
            
            // Validate form data
            if (!formData.name || !formData.academicYear || !formData.semester) {
                setError('Please fill in all required fields');
                return;
            }

            if (formData.feeStructureItems.length === 0) {
                setError('Please add at least one fee item');
                return;
            }

            // Validate fee items
            for (let item of formData.feeStructureItems) {
                if (!item.feeCategoryId || item.amount <= 0) {
                    setError('Please fill in all fee item details');
                    return;
                }
            }

            if (showEditModal && selectedStructure) {
                // Update existing structure
                await feeManagementService.updateFeeStructure(selectedStructure.id, formData);
            } else {
                // Create new structure
                await feeManagementService.createFeeStructure(formData);
            }

            await loadData();
            setShowCreateModal(false);
            setShowEditModal(false);
            setSelectedStructure(null);
            setError(null);
        } catch (err) {
            setError('Failed to save fee structure');
            console.error('Error saving fee structure:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleDeleteStructure = async (structureId) => {
        if (!window.confirm('Are you sure you want to delete this fee structure?')) {
            return;
        }

        try {
            setLoading(true);
            await feeManagementService.deleteFeeStructure(structureId);
            await loadData();
        } catch (err) {
            setError('Failed to delete fee structure');
            console.error('Error deleting fee structure:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleViewInactiveStructures = async () => {
        try {
            setLoading(true);
            const structures = await feeManagementService.getAllFeeStructuresIncludingInactive();
            setAllFeeStructures(structures);
            setShowInactiveModal(true);
        } catch (err) {
            setError('Failed to load inactive fee structures');
            console.error('Error loading inactive structures:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleReactivateStructure = async (structureId) => {
        if (!window.confirm('Are you sure you want to reactivate this fee structure?')) {
            return;
        }

        try {
            setLoading(true);
            await feeManagementService.reactivateFeeStructure(structureId);
            await loadData();
            // Refresh the inactive structures list
            const structures = await feeManagementService.getAllFeeStructuresIncludingInactive();
            setAllFeeStructures(structures);
            alert('Fee structure reactivated successfully!');
        } catch (err) {
            setError('Failed to reactivate fee structure');
            console.error('Error reactivating fee structure:', err);
        } finally {
            setLoading(false);
        }
    };

    if (loading && feeStructures.length === 0) {
        return (
            <div className="flex justify-center items-center h-64">
                <Spinner />
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-3xl font-bold text-gray-900">Fee Structure Management</h1>
                <div className="space-x-2">
                    <Button onClick={handleViewInactiveStructures} variant="outline">
                        View Inactive Structures
                    </Button>
                <Button onClick={handleCreateStructure} variant="primary">
                    Create New Fee Structure
                </Button>
                </div>
            </div>

            {error && (
                <Alert type="error" message={error} />
            )}

            {/* Fee Structures List */}
            <Card>
                <h2 className="text-xl font-semibold mb-4">Fee Structures</h2>
                {feeStructures.length === 0 ? (
                    <p className="text-gray-500">No fee structures found</p>
                ) : (
                    <div className="space-y-4">
                        {feeStructures.map((structure) => (
                            <div key={structure.id} className="border border-gray-200 rounded-lg p-4">
                                <div className="flex justify-between items-start mb-3">
                                    <div>
                                        <h3 className="text-lg font-semibold text-gray-900">{structure.name}</h3>
                                        <p className="text-gray-600">{structure.description}</p>
                                        <div className="flex items-center space-x-4 text-sm text-gray-500 mt-1">
                                            <span>{structure.academicYear} - {structure.semester}</span>
                                            <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                                                structure.isActive 
                                                    ? 'bg-green-100 text-green-800' 
                                                    : 'bg-red-100 text-red-800'
                                            }`}>
                                                {structure.isActive ? 'Active' : 'Inactive'}
                                            </span>
                                        </div>
                                    </div>
                                    <div className="text-right">
                                        <p className="text-2xl font-bold text-green-600">
                                            ${structure.totalAmount.toLocaleString()}
                                        </p>
                                        <p className="text-sm text-gray-500">
                                            {structure.feeStructureItems.length} fee items
                                        </p>
                                    </div>
                                </div>

                                {/* Fee Items Preview */}
                                <div className="grid grid-cols-1 md:grid-cols-2 gap-2 mb-3">
                                    {structure.feeStructureItems.slice(0, 4).map((item) => (
                                        <div key={item.id} className="flex justify-between items-center text-sm">
                                            <span className="text-gray-600">{item.feeCategoryName}</span>
                                            <span className="font-medium">${item.amount.toLocaleString()}</span>
                                        </div>
                                    ))}
                                    {structure.feeStructureItems.length > 4 && (
                                        <div className="text-sm text-gray-500">
                                            +{structure.feeStructureItems.length - 4} more items
                                        </div>
                                    )}
                                </div>

                                {/* Action Buttons */}
                                <div className="flex space-x-2">
                                    <Button 
                                        onClick={() => handleEditStructure(structure)}
                                        variant="outline"
                                        size="sm"
                                    >
                                        Edit
                                    </Button>
                                    <Button 
                                        onClick={() => handleDeleteStructure(structure.id)}
                                        variant="outline"
                                        size="sm"
                                        className="text-red-600 hover:text-red-700"
                                    >
                                        Delete
                                    </Button>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </Card>

            {/* Create/Edit Modal */}
            <Modal
                isOpen={showCreateModal || showEditModal}
                onClose={() => {
                    setShowCreateModal(false);
                    setShowEditModal(false);
                    setSelectedStructure(null);
                }}
                title={showEditModal ? 'Edit Fee Structure' : 'Create New Fee Structure'}
                size="lg"
            >
                <div className="space-y-4">
                    {/* Basic Information */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                Structure Name *
                            </label>
                            <input
                                type="text"
                                value={formData.name}
                                onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                placeholder="e.g., Standard Fall 2025"
                            />
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                Academic Year *
                            </label>
                            <select
                                value={formData.academicYear}
                                onChange={(e) => setFormData(prev => ({ ...prev, academicYear: e.target.value }))}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                            >
                                <option value="">Select Academic Year</option>
                                {availableAcademicYears.map((year) => (
                                    <option key={year.value} value={year.value}>
                                        {year.label}
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                Semester *
                            </label>
                            <select
                                value={formData.semester}
                                onChange={(e) => setFormData(prev => ({ ...prev, semester: e.target.value }))}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                            >
                                <option value="">Select Semester</option>
                                {availableSemesters.map((semester) => (
                                    <option key={semester.value} value={semester.value}>
                                        {semester.label}
                                    </option>
                                ))}
                            </select>
                        </div>
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-1">
                                Description
                            </label>
                            <input
                                type="text"
                                value={formData.description}
                                onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                placeholder="Brief description of the fee structure"
                            />
                        </div>
                    </div>

                    {/* Active Status */}
                    <div className="flex items-center">
                        <label className="flex items-center">
                            <input
                                type="checkbox"
                                checked={formData.isActive}
                                onChange={(e) => setFormData(prev => ({ ...prev, isActive: e.target.checked }))}
                                className="mr-2"
                            />
                            <span className="text-sm font-medium text-gray-700">Active</span>
                        </label>
                        <span className="ml-2 text-sm text-gray-500">
                            {formData.isActive ? 'This fee structure will be available for assignment' : 'This fee structure will be hidden from assignment'}
                        </span>
                    </div>

                    {/* Fee Items */}
                    <div>
                        <div className="flex justify-between items-center mb-3">
                            <h3 className="text-lg font-medium text-gray-900">Fee Items</h3>
                            <Button onClick={handleAddFeeItem} variant="outline" size="sm">
                                Add Fee Item
                            </Button>
                        </div>

                        {formData.feeStructureItems.length === 0 ? (
                            <p className="text-gray-500 text-center py-4">No fee items added yet</p>
                        ) : (
                            <div className="space-y-3">
                                {formData.feeStructureItems.map((item, index) => (
                                    <div key={index} className="border border-gray-200 rounded-lg p-3">
                                        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                                            <div>
                                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                                    Fee Category *
                                                </label>
                                                <select
                                                    value={item.feeCategoryId}
                                                    onChange={(e) => handleFeeItemChange(index, 'feeCategoryId', e.target.value)}
                                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                                >
                                                    <option value="">Select Category</option>
                                                    {feeCategories.map((category) => (
                                                        <option key={category.id} value={category.id}>
                                                            {category.name}
                                                        </option>
                                                    ))}
                                                </select>
                                            </div>
                                            <div>
                                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                                    Amount *
                                                </label>
                                                <input
                                                    type="number"
                                                    value={item.amount}
                                                    onChange={(e) => handleFeeItemChange(index, 'amount', parseFloat(e.target.value) || 0)}
                                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                                    placeholder="0.00"
                                                    min="0"
                                                    step="0.01"
                                                />
                                            </div>
                                            <div>
                                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                                    Due Date
                                                </label>
                                                <input
                                                    type="date"
                                                    value={item.dueDate}
                                                    onChange={(e) => handleFeeItemChange(index, 'dueDate', e.target.value)}
                                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                                />
                                            </div>
                                        </div>
                                        <div className="grid grid-cols-1 md:grid-cols-2 gap-3 mt-3">
                                            <div>
                                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                                    Description
                                                </label>
                                                <input
                                                    type="text"
                                                    value={item.description}
                                                    onChange={(e) => handleFeeItemChange(index, 'description', e.target.value)}
                                                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                                                    placeholder="Optional description"
                                                />
                                            </div>
                                            <div className="flex items-center justify-between">
                                                <label className="flex items-center">
                                                    <input
                                                        type="checkbox"
                                                        checked={item.isRequired}
                                                        onChange={(e) => handleFeeItemChange(index, 'isRequired', e.target.checked)}
                                                        className="mr-2"
                                                    />
                                                    <span className="text-sm font-medium text-gray-700">Required</span>
                                                </label>
                                                <Button
                                                    onClick={() => handleRemoveFeeItem(index)}
                                                    variant="outline"
                                                    size="sm"
                                                    className="text-red-600 hover:text-red-700"
                                                >
                                                    Remove
                                                </Button>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    {/* Total Amount */}
                    <div className="bg-gray-50 p-3 rounded-lg">
                        <div className="flex justify-between items-center">
                            <span className="text-lg font-medium text-gray-900">Total Amount:</span>
                            <span className="text-2xl font-bold text-green-600">
                                ${formData.feeStructureItems.reduce((sum, item) => sum + (item.amount || 0), 0).toLocaleString()}
                            </span>
                        </div>
                    </div>

                    {/* Modal Actions */}
                    <div className="flex justify-end space-x-3 pt-4">
                        <Button
                            onClick={() => {
                                setShowCreateModal(false);
                                setShowEditModal(false);
                                setSelectedStructure(null);
                            }}
                            variant="outline"
                        >
                            Cancel
                        </Button>
                        <Button
                            onClick={handleSubmit}
                            variant="primary"
                            disabled={loading}
                        >
                            {loading ? 'Saving...' : (showEditModal ? 'Update Structure' : 'Create Structure')}
                        </Button>
                    </div>
                </div>
            </Modal>

            {/* Inactive Structures Modal */}
            <Modal
                isOpen={showInactiveModal}
                onClose={() => setShowInactiveModal(false)}
                title="Inactive Fee Structures"
                size="lg"
            >
                <div className="space-y-4">
                    <p className="text-gray-600">
                        The following fee structures are currently inactive and not visible in the main list.
                    </p>
                    
                    {allFeeStructures.filter(s => !s.isActive).length === 0 ? (
                        <p className="text-gray-500 text-center py-4">No inactive fee structures found</p>
                    ) : (
                        <div className="space-y-3">
                            {allFeeStructures.filter(s => !s.isActive).map((structure) => (
                                <div key={structure.id} className="border border-gray-200 rounded-lg p-3">
                                    <div className="flex justify-between items-start mb-2">
                                        <div>
                                            <h4 className="font-medium text-gray-900">{structure.name}</h4>
                                            <p className="text-sm text-gray-600">{structure.description}</p>
                                            <p className="text-xs text-gray-500">
                                                {structure.academicYear} - {structure.semester}
                                            </p>
                                        </div>
                                        <div className="text-right">
                                            <p className="font-semibold text-green-600">
                                                ${structure.totalAmount.toLocaleString()}
                                            </p>
                                            <p className="text-xs text-gray-500">
                                                {structure.feeStructureItems.length} items
                                            </p>
                                        </div>
                                    </div>
                                    <div className="flex justify-end">
                                        <Button
                                            onClick={() => handleReactivateStructure(structure.id)}
                                            variant="outline"
                                            size="sm"
                                            className="text-green-600 hover:text-green-700"
                                        >
                                            Reactivate
                                        </Button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}

                    <div className="flex justify-end pt-4">
                        <Button
                            onClick={() => setShowInactiveModal(false)}
                            variant="outline"
                        >
                            Close
                        </Button>
                    </div>
                </div>
            </Modal>
        </div>
    );
};

export default FeeStructureManager; 