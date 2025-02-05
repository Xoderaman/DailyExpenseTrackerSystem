namespace DailyExpenseTracker.common
{
    public static class StoredProcedure
    {


        public static string GetAllExpensesProcedure = "sp_GetAllExpenses";
        public static string GetExpenseByIdProcedure = "sp_GetExpenseById";
        public static string AddExpenseProcedure = "sp_AddExpense";
        public static string UpdateExpenseProcedure = "sp_UpdateExpense";
        public static string DeleteExpenseProcedure = "sp_DeleteExpense";
        public static string GetUserByEmailProcedure = "GetUserByEmail";
        public static string GetUserByIdProcedure = "GetUserById";
        public static string AddUserProcedure = "AddUser";
        public static string GetExpenseByUserAndExpenseIdProcedure = "GetExpenseByUserAndExpenseIdProcedure";
        public static string GetExpenseDetailByIdProcedure = "sp_GetExpenseDetailById";
        public static string UpdateUserProcedure = "UpdateUser";
        // public static string GetAllExpensesProcedure = "sp_GetAllExpenses";
        //  public static string GetExpenseByIdProcedure = "sp_GetExpenseById";
        // public static string AddExpenseProcedure = "sp_AddExpense";
        // public static string UpdateExpenseProcedure = "sp_UpdateExpense";
        //public static string DeleteExpenseProcedure = "sp_DeleteExpense";



    }
}

