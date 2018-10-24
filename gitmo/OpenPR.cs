using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitmoSharp;

namespace gitmo
{
    public class OpenPR
    {
        Dictionary<string, string> values;

        public OpenPR (Dictionary<string, string> values)
        {
            if (values == null)
                throw new ArgumentNullException (nameof (values));

            this.values = values;
        }

        public async Task Process()
        {
            Console.WriteLine ("Opening Pull Request ...");

            var gitmo = new Gitmo (values["repopath"], values["name"], values["email"]);
            var prUrl = await gitmo.OpenGithubPullRequestAsync (
                values["repoowner"],
                values["reponame"],
                values["branch"],
                values["username"],
                values["pass"],
                values["title"],
                values["message"]
            );

            Console.WriteLine ($"Success! {prUrl}");
        }
    }
}
