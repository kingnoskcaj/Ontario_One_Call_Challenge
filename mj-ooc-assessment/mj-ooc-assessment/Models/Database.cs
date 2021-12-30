/*
*	FILE : Database.cs
*	PROJECT : Ontario One Call Challenge
*	PROGRAMMER : Mark Jackson
*	FIRST VERSION : 2021-12-29
*	DESCRIPTION :
*		this file contains all of the access to the database
*		unfortunately, CalculateCompliance didn't work properly at submission time
*		I know what the problem is but ran out of time to take care of it.
*/
using mj_ooc_challenge.Pages;
using MySql.Data.MySqlClient;
using static mj_ooc_challenge.Classes.results;

namespace mj_ooc_challenge.Classes {
    public class Database {
        /*
        * METHOD : PopulateTree
        * DESCRIPTION :
        *	get the master_codes and their associated member_codes from the database
        * PARAMETERS :
        *	string connection : the connection string for the database we want to connect to
        * RETURNS :
        *	List<Tree> : this is a custom class that where each object has a master_code and a list of member_codes
        *	             they are returned in a list so that the front end can render the list view
        */
        public static List<Tree> PopulateTree(string connection) {
            List<Tree> list = new List<Tree>();         /*list of trees that we will be returning*/
            var con = new MySqlConnection(connection);  /*connection information*/
            
            con.Open();

            /*make sure that we are connected to the database before we work with the data*/
            if (con != null) {
                Tree tempTree = null;   /*tree used to temporarily hold the data while we are parsing it out*/

                /*we want to get all the member_codes and master_codes from the ticket_info table*/
                var statement = "select ticket_info.master_code, ticket_info.member_code from ticket_info group by ticket_info.member_code order by ticket_info.master_code, ticket_info.member_code asc;";
                var command = new MySqlCommand(statement, con);
                var result = command.ExecuteReader();
                while (result.Read()) {
                    string masterCode = (string)result["master_code"];
                    string memberCode = (string)result["member_code"];
                    /*we asked the data to be grouped by master_code from the database. when the master_code changed, we know that there aren't any more for that one*/
                    if (tempTree == null || tempTree.master_code != masterCode) {
                        tempTree = new Tree(masterCode);
                        list.Add(tempTree); /*once there are no more items for this master_code, add its tree the the list*/
                    }
                    tempTree.AddMember(memberCode);
                }
                con.Close();
            }
            return(list);
        }

        /*
        * METHOD : FindTickets
        * DESCRIPTION :
        *	find all the tickets for the selected member_codes
        * PARAMETERS :
        *	string connection : the connection string for the database we want to connect to
        *	string memberCodes : a string of all the member codes we want to find the tickets for
        *	                     it is comma delimited
        * RETURNS :
        *	List<string> : this is a list of all the tickets
        */
        public static List<string> FindTickets(string connection, string memberCodes) {
            List<string> listOfTickets = new List<string>();    /*we will put the ticket numbers here*/
            string[] selection = memberCodes.Split(',');        /*break the passed member_codes at the commas*/

            /*for each member_code that we found*/
            foreach (string option in selection) {
                var con = new MySqlConnection(connection);  /*connection information*/
                con.Open();

                /*make sure we are connected to the database before we attempt to work the data*/
                if (con != null) {
                    /*I know that there is potential for SQL injection in this location*/
                    /*the user could modify the source of the page to change the id of the checkboxes*/
                    /*a solution is to check that all the selected member_codes are ones that we know exist*/
                    var statement = "select ticket_info.ticket_number from ticket_info where ticket_info.member_code = '" + option + "';";
                    var command = new MySqlCommand(statement, con);
                    var result = command.ExecuteReader();
                    while (result.Read()) {
                        string ticket_number = (string)result["ticket_number"];
                        listOfTickets.Add(ticket_number);
                    }
                    con.Close();
                }
            }
            return (listOfTickets);
        }

        /*
        * METHOD : CalculateCompliance
        * DESCRIPTION :
        *	determine how many of the tickets were compliant. some of this could be done using the database but I wasn't sure how to achieve this.
        *	most of the performance is lost in this section
        * PARAMETERS :
        *	string connection : the connection string for the database we want to connect to
        *	List<string> listOfTickets : a list containing all of the tickets we want to find the compliance values for
        * RETURNS :
        *	none
        *	IndexModel.resultGrid is modified
        */
        public static void CalculateCompliance(string connection, List<string> listOfTickets) {
            /*for each of the passed tickets*/
            foreach (string option in listOfTickets) {

                var con = new MySqlConnection(connection); /*connection information*/ 
                con.Open();

                /*make sure we are connected to the database before attempting to use the data*/
                if (con != null) {

                    /*call the stored procedure with the current ticket as the parameter*/
                    var statement = "call populate_data('" + option + "');";
                    var command = new MySqlCommand(statement, con);
                    command.ExecuteNonQuery();

                    /*select the time_to_respond and the compliance from the database for the ticket.
                     * yes we just generated this. I probably should have had the procedure return it but I wasn't
                     * confidant in my abilities*/
                    statement = "select ticket_info.time_to_respond, ticket_info.compliance from ticket_info where ticket_info.ticket_number = '" + option + "';";
                    command = new MySqlCommand(statement, con);
                    var result = command.ExecuteReader();
                    while (result.Read()) {
                        string responseTime = (string)result["time_to_respond"];
                        UInt64 compliant = (UInt64)result["compliance"];
                        int index = -1;
                        /*find what the response time was and set index to the index of the response*/
                        /*I tried to use a switch here but visual studio was very unhappy with my constants*/
                        if (responseTime == ResultGrid.titles[0]) {
                            index = 0;
                        } else if (responseTime == ResultGrid.titles[1]) {
                            index = 1;
                        } else if (responseTime == ResultGrid.titles[2]) {
                            index = 2;
                        } else if (responseTime == ResultGrid.titles[3]) {
                            index = 3;
                        } else {
                            break;
                        }

                        /*check if the result had compliant set to 1 or 0. 1 if compliant 0 if not.
                         * increase the individual compliance counts*/
                        if (compliant == 1) {
                            IndexModel.resultGrid.results[index].compliant += 1;
                        } else {
                            IndexModel.resultGrid.results[index].noncompliant += 1;
                        }
                        /*increase the total counts*/
                        IndexModel.resultGrid.results[index].count++;
                        IndexModel.resultGrid.totalCount++;
                    }
                    con.Close();
                }
            }
        }
    }
}
