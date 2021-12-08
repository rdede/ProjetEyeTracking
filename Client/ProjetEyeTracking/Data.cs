using System;
using System.Collections.Generic;



namespace ProjetEyeTracking
{
    public class StartedAt
    {
        public double x { get; set; }
        public double y { get; set; }
    }

    public class EndedAt
    {
        public double x { get; set; }
        public double y { get; set; }
    }

    public class Fixation
    {
        public StartedAt startedAt { get; set; }
        public EndedAt endedAt { get; set; }
        public TimeSpan duration { get; set; }
    }

    public class Page
    {
        public int pageNb { get; set; }
        public string imgSelect { get; set; }
        public IList<Fixation> fixations { get; set; }
    }

    public class Data
    {
        public string idUser { get; set; }
        public List<Page> pages { get; set; }
    }

}

