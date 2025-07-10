// Script to create user accounts for existing students
import { userService } from '../services/userService.js';

const existingStudents = [
  {
    username: 'alex.student',
    email: 'alex.mutahi@xyzuniversity.edu',
    fullName: 'Alex Mutahi',
    password: 'Alex123!',
    defaultRole: 'Student'
  },
  {
    username: 'john.student',
    email: 'john.doe@xyzuniversity.edu',
    fullName: 'John Doe',
    password: 'John123!',
    defaultRole: 'Student'
  },
  {
    username: 'sarah.student',
    email: 'sarah.johnson@xyzuniversity.edu',
    fullName: 'Sarah Johnson',
    password: 'Sarah123!',
    defaultRole: 'Student'
  },
  {
    username: 'mike.student',
    email: 'mike.wilson@xyzuniversity.edu',
    fullName: 'Mike Wilson',
    password: 'Mike123!',
    defaultRole: 'Student'
  },
  {
    username: 'andrew.student',
    email: 'andrew.smith@xyzuniversity.edu',
    fullName: 'Andrew Smith',
    password: 'Andrew123!',
    defaultRole: 'Student'
  }
];

const createStudentUsers = async () => {
  console.log('Creating user accounts for existing students...');
  
  for (const student of existingStudents) {
    try {
      console.log(`Creating user for ${student.fullName}...`);
      const result = await userService.createUser(student);
      console.log(`✅ Created user: ${student.username} (ID: ${result.id})`);
    } catch (error) {
      if (error.response?.status === 400 && error.response?.data?.message?.includes('already exists')) {
        console.log(`⚠️  User ${student.username} already exists, skipping...`);
      } else {
        console.error(`❌ Failed to create user ${student.username}:`, error.response?.data?.message || error.message);
      }
    }
  }
  
  console.log('Finished creating student users!');
};

// Run the script
createStudentUsers(); 