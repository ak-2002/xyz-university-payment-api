
using xyz_university_payment_api.Models;

namespace xyz_university_payment_api.Interfaces
{
   
   
    public interface IPaymentRepository : IRepository<PaymentNotification>
    {
        
        Task<IEnumerable<PaymentNotification>> GetPaymentsByStudentAsync(string studentNumber);


        Task<IEnumerable<PaymentNotification>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);

       
        Task<PaymentNotification?> GetByPaymentReferenceAsync(string paymentReference);

        /// <summary>
        /// Checks if a payment reference already exists
        /// Important for preventing duplicate payment processing
        /// </summary>
        /// <param name="paymentReference">The payment reference to check</param>
        /// <returns>True if the payment reference exists, false otherwise</returns>
        Task<bool> PaymentReferenceExistsAsync(string paymentReference);

        /// <summary>
        /// Retrieves the total amount paid by a student
        /// Useful for financial calculations and reporting
        /// </summary>
        /// <param name="studentNumber">The student number</param>
        /// <returns>The total amount paid by the student</returns>
        Task<decimal> GetTotalAmountPaidByStudentAsync(string studentNumber);

        /// <summary>
        /// Retrieves payments above a certain amount
        /// Useful for identifying large payments or setting thresholds
        /// </summary>
        /// <param name="minimumAmount">The minimum amount threshold</param>
        /// <returns>Collection of payments above the specified amount</returns>
        Task<IEnumerable<PaymentNotification>> GetPaymentsAboveAmountAsync(decimal minimumAmount);

        /// <summary>
        /// Retrieves the most recent payment for a student
        /// Useful for checking latest payment status
        /// </summary>
        /// <param name="studentNumber">The student number</param>
        /// <returns>The most recent payment for the student, null if no payments exist</returns>
        Task<PaymentNotification?> GetLatestPaymentByStudentAsync(string studentNumber);

        /// <summary>
        /// Retrieves payments that were received on a specific date
        /// Useful for daily reconciliation and reporting
        /// </summary>
        /// <param name="dateReceived">The date when payments were received</param>
        /// <returns>Collection of payments received on the specified date</returns>
        Task<IEnumerable<PaymentNotification>> GetPaymentsByDateReceivedAsync(DateTime dateReceived);
    }
} 