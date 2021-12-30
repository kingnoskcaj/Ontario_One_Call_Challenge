/*
*	FILE : results.cs
*	PROJECT : Ontario One Call Challenge
*	PROGRAMMER : Mark Jackson
*	FIRST VERSION : 2021-12-29
*	DESCRIPTION :
*		this file contains the definition for how results are stored as well as a definition for how the
*		data is displayed in the results grid.
*/
namespace mj_ooc_challenge.Classes {
    public class results {
        public class Result {
            public string response_time { get; set; }
            public string percentage { get; set; }
            public UInt64 compliant { get; set; }
            public UInt64 noncompliant { get; set; }
            public int count { get; set; }
        }
        public class ResultGrid {
            public Result[] results;
            public int totalCount;
            public int totalCompliant;
            public int totalNoncompliant;
            public static readonly string[] titles = { "0-5", "5-10", "11-15", "15+" };
            public ResultGrid() {
                results = new Result[4];
                for (int i = 0; i < results.Length; i++) {
                    results[i] = new Result();
                    results[i].response_time = titles[i];
                    results[i].percentage = "0%";
                    results[i].compliant = 0;
                    results[i].noncompliant = 0;
                    results[i].count = 0;
                    totalCount = 0;
                    totalCompliant = 0;
                    totalNoncompliant = 0;
                }
            }
        }
    }
}
