using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Drawing;
using ColorTrackerLib;


namespace Test
{
	public partial class SettingsControl : UserControl
	{
		public ObservableCollection<Settings> SettingsList { get; private set; }

		private List<MarkerSettings> _markerSettings;
		private Settings _currentSettings;

		public SettingsControl()
		{
			
		}

		public SettingsControl(Capture capture, bool editable)
		{
			InitializeComponent();

			if (editable)
			{
				textBox1.Enabled = true;
				button1.Enabled = true;
				dataGridView1.Columns[1].Visible = true;
			}

			_markerSettings = capture.Scanner.MarkerSettings;
			SettingsList = new ObservableCollection<Settings>();
			SettingsList.CollectionChanged += SettingsList_CollectionChanged;
		}

		void SettingsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (Settings settings in e.NewItems)
					{
						DataGridViewRow newRow = new DataGridViewRow();
						newRow.CreateCells(dataGridView1);
						newRow.Cells[0].Value = settings.Name;
						dataGridView1.Rows.Add(newRow);
						_markerSettings.Add(settings.Marker);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (Settings settings in e.OldItems)
					{
						for (int i = 0; i < dataGridView1.Rows.Count; i++)
							if (settings.Name == dataGridView1.Rows[i].Cells[0].Value.ToString())
							{
								dataGridView1.Rows.RemoveAt(i);
								_markerSettings.Remove(settings.Marker);
								break;
							}
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Reset:
					dataGridView1.Rows.Clear();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void trackBar1_ValueChanged(object sender, EventArgs e)
		{
			trackBar1.BackColor = Hsv.ToColor(new Hsv(trackBar1.Value, 1, 1));
			label6.Text = trackBar1.Value.ToString();

			if (_currentSettings != null)
				_currentSettings.Marker.AverageH = trackBar1.Value;
		}

		private void trackBar2_ValueChanged(object sender, EventArgs e)
		{
			label7.Text = trackBar2.Value.ToString();

			if (_currentSettings != null)
				_currentSettings.Marker.MaxDifH = trackBar2.Value;
		}

		private void trackBar3_ValueChanged(object sender, EventArgs e)
		{
			if (trackBar3.Value > trackBar4.Value)
				trackBar3.Value = trackBar4.Value;
			label8.Text = trackBar3.Value.ToString();

			if (_currentSettings != null)
				_currentSettings.Marker.MinS = trackBar3.Value / 100f;
		}

		private void trackBar4_ValueChanged(object sender, EventArgs e)
		{
			if (trackBar3.Value > trackBar4.Value)
				trackBar4.Value = trackBar3.Value;
			label9.Text = trackBar4.Value.ToString();

			if (_currentSettings != null)
				_currentSettings.Marker.MaxS = trackBar4.Value / 100f;
		}

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
                return;

            foreach (DataGridViewRow item in dataGridView1.Rows)
                if (item.Cells[0].Value.ToString() == textBox1.Text)
                    return;

			Settings newSettings = new Settings(textBox1.Text);
			SettingsList.Add(newSettings);
			_markerSettings.Add(newSettings.Marker);


			DataGridViewRow newRow = new DataGridViewRow();
			newRow.CreateCells(dataGridView1);
			newRow.Cells[0].Value = newSettings.Name;

			dataGridView1.Rows.Add(newRow);

			dataGridView1.CurrentCell = newRow.Cells[0];
	        newRow.Selected = true;

	        textBox1.Text = "";
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
			trackBar1.Enabled = false;
			trackBar2.Enabled = false;
			trackBar3.Enabled = false;
			trackBar4.Enabled = false;

	        if (dataGridView1.CurrentCell != null)
	        {
		        String name = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[0].Value.ToString();

				foreach (Settings settings in SettingsList)
			        if (settings.Name == name)
			        {
				        _currentSettings = settings;
				        break;
			        }

				trackBar1.Value = (int)_currentSettings.Marker.AverageH;
		        trackBar1_ValueChanged(this, e);

				trackBar2.Value = (int)_currentSettings.Marker.MaxDifH;
				trackBar3.Value = (int)(_currentSettings.Marker.MinS * 100);
				trackBar4.Value = (int)(_currentSettings.Marker.MaxS * 100);

				trackBar1.Enabled = true;
				trackBar2.Enabled = true;
				trackBar3.Enabled = true;
				trackBar4.Enabled = true;
	        }
	        else
	        {
				trackBar1.Value = 0;
				trackBar1.BackColor = Color.Empty;
				trackBar2.Value = 0;
				trackBar3.Value = 0;
				trackBar4.Value = 0;
		        
	        }
        }

		private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex != 1)
				return;

			String name = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

			Settings settingsToDelete = null;

			foreach (Settings settings in SettingsList)
				if (settings.Name == name)
				{
					settingsToDelete = settings;
					break;
				}

			if (_currentSettings == settingsToDelete)
				_currentSettings = null;

			SettingsList.Remove(settingsToDelete);

		}

		public sealed class Settings
		{
			public String Name { get; private set; }
			public MarkerSettings Marker { get; private set; }

			public Settings(String name)
			{
				Name = name;
				Marker = new MarkerSettings();
			}
		}
	}
}
