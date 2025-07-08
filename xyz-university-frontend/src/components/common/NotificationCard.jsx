import React from 'react';

const NotificationCard = ({ 
  icon, 
  children, 
  variant = 'info',
  className = '',
  ...props 
}) => {
  const baseClasses = 'flex items-start p-4 rounded-xl transition-all duration-300 ease-in-out transform hover:-translate-y-1 hover:scale-[1.02]';
  
  const variantClasses = {
    info: 'bg-gradient-to-r from-blue-50 to-blue-100/50 text-blue-800 border border-blue-200/50 shadow-sm hover:shadow-blue-500/20',
    warning: 'bg-gradient-to-r from-yellow-50 to-yellow-100/50 text-yellow-800 border border-yellow-200/50 shadow-sm hover:shadow-yellow-500/20',
    success: 'bg-gradient-to-r from-green-50 to-green-100/50 text-green-800 border border-green-200/50 shadow-sm hover:shadow-green-500/20',
    error: 'bg-gradient-to-r from-red-50 to-red-100/50 text-red-800 border border-red-200/50 shadow-sm hover:shadow-red-500/20'
  };

  const classes = `${baseClasses} ${variantClasses[variant]} ${className}`;

  return (
    <div className={classes} {...props}>
      {icon && (
        <div className="flex-shrink-0 mr-4 mt-0.5">
          {React.cloneElement(icon, { 
            className: 'w-6 h-6 transition-transform duration-300 hover:scale-110' // Larger icons with animation
          })}
        </div>
      )}
      <div className="flex-1">
        {children}
      </div>
    </div>
  );
};

export default NotificationCard; 