using System.Collections.Generic;
using System.Data.SqlClient;
using DepartmentsEmployees.Models;

namespace DepartmentsEmployees.Data
{
    /// <summary>
    ///  An object to contain all database interactions.
    /// </summary>
    public class Repository
    {
        /// <summary>
        ///  Represents a connection to the database.
        ///   This is a "tunnel" to connect the application to the database.
        ///   All communication between the application and database passes through this connection.
        /// </summary>
        public SqlConnection Connection
        {
            get
            {
                // This is "address" of the database
                string _connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=DepartmentsEmployees;Integrated Security=True";
                return new SqlConnection(_connectionString);
            }
        }


        /************************************************************************************
         * Departments
         ************************************************************************************/

        /// <summary>
        ///  Returns a list of all departments in the database
        /// </summary>
        public List<Department> GetAllDepartments()
        {
            // We must "use" the database connection.
            //  Because a database is a shared resource (other applications may be using it too) we must
            //  be careful about how we interact with it. Specifically, we Open() connections when we need to
            //  interact with the database and we Close() them when we're finished.
            //  In C#, a "using" block ensures we correctly disconnect from a resource even if there is an error.
            //  For database connections, this means the connection will be properly closed.
            using (SqlConnection conn = Connection)
            {
                // Note, we must Open() the connection, the "using" block doesn't do that for us.
                conn.Open();

                // We must "use" commands too. this creates a scope. at the end it will close the connection
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // Here we setup the command with the SQL we want to execute before we execute it.
                    cmd.CommandText = "SELECT Id, DeptName FROM Department";

                    // Execute the SQL in the database and get a "reader" that will give us access to the data.
                    SqlDataReader reader = cmd.ExecuteReader();
                    //we have to create the list(container) that holds that data we got back
                    // A list to hold the departments we retrieve from the database.
                    List<Department> departments = new List<Department>();

                    // Read() will return true if there's more data to read
                    while (reader.Read())
                    {
                        // The "ordinal" is the numeric position of the column in the query results.
                        //  For our query, "Id" has an ordinal value of 0 and "DeptName" is 1.
                        int idColumnPosition = reader.GetOrdinal("Id");
                        //ok this ID is an integer. So i need to use getInt32. if a string then getstring.
                        //my responsibility to call correct method and know the type

                        // We user the reader's GetXXX methods to get the value for a particular ordinal.
                        int idValue = reader.GetInt32(idColumnPosition);

                        int deptNameColumnPosition = reader.GetOrdinal("DeptName");
                        string deptNameValue = reader.GetString(deptNameColumnPosition);

                        // Now let's create a new department object using the data from the database.
                        Department department = new Department
                        {
                            Id = idValue,
                            DeptName = deptNameValue
                        };

                        // ...and add that department object to our list.
                        departments.Add(department);
                    }

                    // We should Close() the reader. Unfortunately, a "using" block won't work here.
                    reader.Close();

                    // Return the list of departments who whomever called this method.
                    return departments;
                }
            }
        }

        /// <summary>
        ///  Returns a single department with the given id.
        /// </summary>
        public Department GetDepartmentById(int id)
        {
            using (SqlConnection conn = Connection) //at the end of this block we will dispose the connection
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                { //the @ sign means whatever they passed in I want to use it here. variable place holder.
                    cmd.CommandText = "SELECT DeptName FROM Department WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id)); //so we create a new parameter. We're filling in the blank
                    SqlDataReader reader = cmd.ExecuteReader(); //execute the command and get the reader

                    Department department = null; //single department set this to null to instantiate it and if it doesn't come back it will be null
                    if (reader.Read()) //if we don't get a record back it will be false
                    {
                        department = new Department //this is setting up the design of what it will look like.
                        {
                            Id = id,
                            DeptName = reader.GetString(reader.GetOrdinal("DeptName"))//theres only one column so it will be 0
                        };
                    }

                    reader.Close();

                    return department; //if we don't get anything back we get null (line109)
                }
            }
        }

        /// <summary>
        ///  Add a new department to the database
        ///   NOTE: This method sends data to the database,
        ///   it does not get anything from the database, so there is nothing to return.
        /// </summary>
        public void AddDepartment(Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // These SQL parameters are annoying. Why can't we use string interpolation?
                    // ... sql injection attacks!!!
                    cmd.CommandText = "INSERT INTO Department (DeptName) Values (@deptName)";
                    cmd.Parameters.Add(new SqlParameter("@deptName", department.DeptName));
                    cmd.ExecuteNonQuery(); //catch all for anything that is not select
                }
            }

            // when this method is finished we can look in the database and see the new department.
        }

        /// <summary>
        ///  Updates the department with the given id
        /// </summary>
        public void UpdateDepartment(int id, Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Department
                                           SET DeptName = @deptName
                                         WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@deptName", department.DeptName));
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        ///  Delete the department with the given id
        /// </summary>
        public void DeleteDepartment(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Department WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }


        /************************************************************************************
         * Employees
         ************************************************************************************/

        public List<Employee> GetAllEmployees()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, FirstName, LastName, DepartmentId FROM Employee";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId"))
                        };

                        employees.Add(employee);
                    }

                    reader.Close();

                    return employees;
                }
            }
        }

        /// <summary>
        ///  Get an individual employee by id
        /// </summary>
        /// <param name="id">The employee's id</param>
        /// <returns>The employee that with the given id</returns>
        public Employee GetEmployeeById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                { //the @ sign means whatever they passed in I want to use it here. variable place holder.
                    cmd.CommandText = "SELECT FirstName, LastName FROM Employee WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id)); //so we create a new parameter. We're filling in the blank
                    SqlDataReader reader = cmd.ExecuteReader(); //execute the command and get the reader

                    Employee employee = null; //single department set this to null to instantiate it and if it doesn't come back it will be null
                    if (reader.Read()) //if we don't get a record back it will be false
                    {
                        employee = new Employee //this is setting up the design of what it will look like.
                        {
                            Id = id,
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")), 
                            LastName = reader.GetString(reader.GetOrdinal("LastName"))  
                        };
                    }
                    reader.Close();
                    return null;
                }
            }
        }


        /// <summary>
        ///  Get all employees along with their departments
        /// </summary>
        /// <returns>A list of employees in which each employee object contains their department object.</returns>
        public List<Employee> GetAllEmployeesWithDepartment()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id, e.FirstName, e.LastName, e.DepartmentId,
                                               d.DeptName
                                          FROM Employee e LEFT JOIN Department d ON e.DepartmentID = d.id";
                     
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Employee> employeesWithDepartments = new List<Employee>();
                    // Read() will return true if there's more data to read
                    while (reader.Read())
                    {
                        // The "ordinal" is the numeric position of the column in the query results.
                        //  For our query, "Id" has an ordinal value of 0 and "DeptName" is 1.
                        Employee employee = new Employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")), //give me the integer at column id
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")), //created this column
                            Department = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                DeptName = reader.GetString(reader.GetOrdinal("DeptName"))
                            }
                        };

                        // ...and add that department object to our list.
                        employeesWithDepartments.Add(employee);
                    }

                    // We should Close() the reader. Unfortunately, a "using" block won't work here.
                    reader.Close();

                    // Return the list of departments who whomever called this method.
                    return employeesWithDepartments;
                }
            }
        }


        /// <summary>
        ///  Get employees who are in the given department. Include the employee's department object.
        /// </summary>
        /// <param name="departmentId">Only include employees in this department</param>
        /// <returns>A list of employees in which each employee object contains their department object.</returns>
        public List<Employee> GetAllEmployeesWithDepartmentByDepartmentId(int departmentId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT e.Id, e.FirstName, e.LastName, e.DepartmentId,
                                               d.DeptName
                                          FROM Employee e INNER JOIN Department d ON e.DepartmentID = d.id
                                         WHERE d.id = @departmentId";
                    cmd.Parameters.Add(new SqlParameter("@departmentId", departmentId));
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Employee> employees = new List<Employee>();
                    while (reader.Read()) //iterate over the result set employee and dept name
                    {
                        Employee employee = new Employee //create a new employee
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")), //give me the integer at column id
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")), //created this column
                            Department = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                DeptName = reader.GetString(reader.GetOrdinal("DeptName"))
                            }
                        };

                        employees.Add(employee);
                    }

                    reader.Close();

                    return employees;
                }
            }
        }


        /// <summary>
        ///  Add a new employee to the database
        ///   NOTE: This method sends data to the database,
        ///   it does not get anything from the database, so there is nothing to return.
        /// </summary>
        public void AddEmployee(Employee employee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // These SQL parameters are annoying. Why can't we use string interpolation?
                    // ... sql injection attacks!!!
                    cmd.CommandText = "INSERT INTO Employee (FirstName, LastName, DepartmentId) Values (@FirstName, @LastName, @DepartmentId)";
                    cmd.Parameters.Add(new SqlParameter("@FirstName", employee.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", employee.LastName));
                    cmd.Parameters.Add(new SqlParameter("@DepartmentId", employee.DepartmentId));
                    cmd.ExecuteNonQuery(); //catch all for anything that is not select
                }
            }

        }

        /// <summary>
        ///  Updates the employee with the given id
        /// </summary>
        public void UpdateEmployee(int id, Employee employee)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Employee
                                           SET DepartmentId = @departmentId
                                         WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@departmentId", employee.DepartmentId));
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        ///  Delete the employee with the given id
        /// </summary>
        public void DeleteEmployee(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Employee WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
