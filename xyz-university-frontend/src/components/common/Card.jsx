import React from 'react';

const Card = ({ 
  children, 
  className = '', 
  variant = 'default',
  hover = true,
  ...props 
}) => {
  const baseClasses = 'bg-white rounded-xl border border-gray-100 transition-all duration-300 ease-in-out';
  
  const variantClasses = {
    default: 'shadow-sm hover:shadow-lg hover:shadow-gray-200/50',
    elevated: 'shadow-md hover:shadow-xl hover:shadow-gray-300/50',
    glass: 'backdrop-blur-sm bg-white/80 border-white/20 shadow-lg',
    gradient: 'bg-gradient-to-br from-white to-gray-50/50 shadow-md hover:shadow-lg'
  };

  const hoverClasses = hover ? 'hover:-translate-y-1 hover:scale-[1.02]' : '';
  
  const classes = `${baseClasses} ${variantClasses[variant]} ${hoverClasses} ${className}`;

  return (
    <div className={classes} {...props}>
      {children}
    </div>
  );
};

export default Card; 