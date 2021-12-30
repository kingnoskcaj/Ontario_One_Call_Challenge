/*
*	FILE : Index.cshtml.cs
*	PROJECT : Ontario One Call Challenge
*	PROGRAMMER : Mark Jackson
*	FIRST VERSION : 2021-12-29
*	DESCRIPTION :
*		this file contains the model code for the main view of the program.
*		this could be improved with more models and controllers but alas.
*/
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using mj_ooc_challenge.Classes;
using MySql.Data.MySqlClient;
using static mj_ooc_challenge.Classes.results;

namespace mj_ooc_challenge.Pages {
    public class IndexModel : PageModel {
        public static ResultGrid resultGrid = new ResultGrid(); /*this is used by the front end to display the results of the processing*/
        public List<Tree> codeList = new List<Tree>();          /*this is used to display the listview on the left side*/

        /*get configuration data from the appsettings.json and set up logging*/
        private readonly ILogger<IndexModel> _logger;
        public readonly IConfiguration _configuration;
        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration) {
            _configuration = configuration;
            _logger = logger;
        }

        /*
        * METHOD : OnGet
        * DESCRIPTION :
        *	when the webpage is called with a GET http request, this function will be called.
        *	This function access the database to determine which master and member codes should
        *	be displayed in the list on the left side of the window
        * PARAMETERS :
        *	none
        * RETURNS :
        *	none
        *	codeList is updated
        */
        public void OnGet() {
            codeList = Database.PopulateTree(_configuration.GetConnectionString("mj_ooc_challenge"));
        }

        /*bind this property so we can access data on post*/
        [BindProperty]
        public string selectedOptions { get; set; } /*this string contains a comma delimited version of the checkboxes that were selected*/

        /*
        * METHOD : OnPost
        * DESCRIPTION :
        *	when the webpage is called with a POST http request, this function will be called.
        *	This function access the database to calculate the compliance attributes of the members specified
        *	in the parameter.
        *	some of this could be done using the database but I wasn't sure how to achieve this.
        *	most of the performance is lost in this section
        * PARAMETERS :
        *	string selectedOptions : the comma delimited checkbox selection of member codes
        * RETURNS :
        *	none
        *	resultGrid is updated
        *	codeList is updated
        */
        public void OnPost(string selectedOptions) {
            resultGrid = new ResultGrid();  /*reset the resultGrid*/

            /*find the tickets for the members*/
            List<string> listOfTickets = Database.FindTickets(_configuration.GetConnectionString("mj_ooc_challenge"), selectedOptions);

            /*for each ticket, determine compliance
             * most of this section is the same as CalculateCompliance in Database.cs*/
            foreach (string option in listOfTickets) {

                var con = new MySqlConnection(_configuration.GetConnectionString("mj_ooc_challenge")); /*connection information*/
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
                            resultGrid.results[index].compliant += 1;
                        } else {
                            resultGrid.results[index].noncompliant += 1;
                        }
                        /*increase the total counts*/
                        resultGrid.results[index].count++;
                        resultGrid.totalCount++;
                    }
                    con.Close();
                }
            }

                    
            /*calculate the percentage*/
            for (int i = 0; i < resultGrid.results.Length; i++) {
                int count = resultGrid.results[i].count;
                int total = resultGrid.totalCount;
                float percentage = (float)count / (float)total;
                int percentageAsInt = (int)(percentage * 100);
                resultGrid.results[i].percentage = "" + percentageAsInt + "%";
            }

            /*update the code list on the left side of the window*/
            codeList = Database.PopulateTree(_configuration.GetConnectionString("mj_ooc_challenge"));
        }
    }
}
