using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NugetPack
{
    public class GenAlgoViewer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public GenericAlgo GenAlg = new GenericAlgo();
        public int n_sq1 = 3;
        public int n_sq2 = 2;
        public int n_sq3 = 1;
        int n_iterations = 10000;
        int inter_num = 0;
        int in_it_num = 0;
        int latest_sq = 10000;
        bool is_running = false;
       
        public GenericAlgo Alg;

        public GenAlgoViewer()
        {
            Alg = new GenericAlgo();
            GenAlg.ItUpdated += UpdateNIters;
            GenAlg.SqUpdated += UpdateSq;

            Task.Run(async () =>
            {
                while (true)
                {
                    if (!is_running) continue;
                    await Task.Delay(100);
                    DisplayedIters = in_it_num;
                    DispalayedSq = latest_sq;
                }
            });
        }

        public int N_sqs_1
        { 
            get { return n_sq1; } 
            set {  n_sq1 = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("N_sqs_1"));
            }
        }

        public int N_sqs_2
        {
            get { return n_sq2; }
            set
            {
                n_sq2 = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("N_sqs_2"));
            }
        }

        public int N_sqs_3
        {
            get { return n_sq3; }
            set
            {
                n_sq3 = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("N_sqs_3"));
            }
        }


        public Solution Start(CancellationToken token, Action<int, int, Solution> onProgressUpdate)
        {
            is_running = true;
            return GenAlg.GetBestSol(n_iterations, token, onProgressUpdate);
        }

        private void UpdateNIters(int new_n_iters)
        {
            in_it_num = new_n_iters;
        }


        public void Prepare()
        {
            GenAlg = new GenericAlgo();
            GenAlg.Initialize(n_sq1, n_sq2, n_sq3, 100);
        }


        public int DisplayedIters
        { 
            get { return inter_num; }
            private set
            {
                inter_num = value;
                OnPropertyChanged();
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("DisplayedIters"));
            }
        }



        private void UpdateSq(int new_sq)
        {
            latest_sq = new_sq;
        }

        public int DispalayedSq
        {
            get { return latest_sq; }
            private set
            {
                latest_sq = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
