import React from 'react';

const QuickActionButton = ({ 
  icon, 
  children, 
  variant = 'primary',
  className = '',
  onClick,
  ...props 
}) => {
  const baseClasses = 'w-full flex items-center px-6 py-4 text-left font-semibold rounded-xl transition-all duration-300 ease-in-out hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-offset-2 transform hover:-translate-y-1 hover:scale-[1.02] active:scale-[0.98]';
  
  const variantClasses = {
    primary: 'bg-gradient-to-r from-blue-50 to-blue-100/50 text-blue-700 hover:from-blue-100 hover:to-blue-200/50 focus:ring-blue-500 border border-blue-200/50 hover:border-blue-300/50 shadow-sm hover:shadow-blue-500/20',
    secondary: 'bg-gradient-to-r from-gray-50 to-gray-100/50 text-gray-700 hover:from-gray-100 hover:to-gray-200/50 focus:ring-gray-500 border border-gray-200/50 hover:border-gray-300/50 shadow-sm hover:shadow-gray-500/20',
    success: 'bg-gradient-to-r from-green-50 to-green-100/50 text-green-700 hover:from-green-100 hover:to-green-200/50 focus:ring-green-500 border border-green-200/50 hover:border-green-300/50 shadow-sm hover:shadow-green-500/20',
    warning: 'bg-gradient-to-r from-yellow-50 to-yellow-100/50 text-yellow-700 hover:from-yellow-100 hover:to-yellow-200/50 focus:ring-yellow-500 border border-yellow-200/50 hover:border-yellow-300/50 shadow-sm hover:shadow-yellow-500/20',
    danger: 'bg-gradient-to-r from-red-50 to-red-100/50 text-red-700 hover:from-red-100 hover:to-red-200/50 focus:ring-red-500 border border-red-200/50 hover:border-red-300/50 shadow-sm hover:shadow-red-500/20'
  };

  const classes = `${baseClasses} ${variantClasses[variant]} ${className}`;

  return (
    <button
      className={classes}
      onClick={onClick}
      {...props}
    >
      {icon && (
        <div className="flex-shrink-0 mr-4">
          {React.cloneElement(icon, { 
            className: 'w-6 h-6 transition-transform duration-300 group-hover:scale-110' // Larger icons with animation
          })}
        </div>
      )}
      <span className="flex-1 text-base">{children}</span>
      <div className="flex-shrink-0 ml-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
        </svg>
      </div>
    </button>
  );
};

export default QuickActionButton; 