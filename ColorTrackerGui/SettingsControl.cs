using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;
using ColorTrackerLib;

namespace ColorTrackerGui
{
	public partial class SettingsControl : UserControl
	{
		public ObservableCollection<SettingsWrapper> SettingsList { get; }

		private readonly List<MarkerSettings> _markerSettings;
		private SettingsWrapper _currentSettingsWrapper;

		public SettingsControl(List<MarkerSettings> markerSettings, bool editable)
		{
			InitializeComponent();

			if (editable)
			{
				textBox1.Enabled = true;
				button1.Enabled = true;
				dataGridView1.Columns[1].Visible = true;
			}

			_markerSettings = markerSettings;
			SettingsList = new ObservableCollection<SettingsWrapper>();
			SettingsList.CollectionChanged += SettingsList_CollectionChanged;
		}

		void SettingsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (SettingsWrapper settings in e.NewItems)
					{
						DataGridViewRow newRow = new DataGridViewRow();
						newRow.CreateCells(dataGridView1);
						newRow.Cells[0].Value = settings.Name;
						dataGridView1.Rows.Add(newRow);
						_markerSettings.Add(settings.Settings);
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (SettingsWrapper settings in e.OldItems)
					{
						for (int i = 0; i < dataGridView1.Rows.Count; i++)
							if (settings.Name == dataGridView1.Rows[i].Cells[0].Value.ToString())
							{
								dataGridView1.Rows.RemoveAt(i);
								_markerSettings.Remove(settings.Settings);
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
			_currentSettingsWrapper.Settings.AverageH = trackBar1.Value;
			trackBar1.BackColor = Hsv.ToColor(new Hsv(trackBar1.Value, 1, 1));
			label6.Text = _currentSettingsWrapper.Settings.AverageH.ToString();
		}

		private void trackBar2_ValueChanged(object sender, EventArgs e)
		{
			_currentSettingsWrapper.Settings.MaxDifH = trackBar2.Value;
			label7.Text = _currentSettingsWrapper.Settings.MaxDifH.ToString();
		}

		private void trackBar3_ValueChanged(object sender, EventArgs e)
		{
			_currentSettingsWrapper.Settings.MinS = trackBar3.Value / 100f;
			label8.Text = _currentSettingsWrapper.Settings.MinS.ToString();
		}

		private void trackBar4_ValueChanged(object sender, EventArgs e)
		{
			_currentSettingsWrapper.Settings.MaxS = trackBar4.Value / 100f;
			label9.Text = _currentSettingsWrapper.Settings.MaxS.ToString();
		}

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
                return;

            foreach (DataGridViewRow item in dataGridView1.Rows)
                if (item.Cells[0].Value.ToString() == textBox1.Text)
                    return;

			SettingsWrapper newSettingsWrapper = new SettingsWrapper(textBox1.Text, new MarkerSettings());
			SettingsList.Add(newSettingsWrapper);
			_markerSettings.Add(newSettingsWrapper.Settings);


			DataGridViewRow newRow = new DataGridViewRow();
			newRow.CreateCells(dataGridView1);
			newRow.Cells[0].Value = newSettingsWrapper.Name;

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

				foreach (SettingsWrapper settings in SettingsList)
			        if (settings.Name == name)
			        {
				        _currentSettingsWrapper = settings;
				        break;
			        }

				trackBar1.Value = (int)_currentSettingsWrapper.Settings.AverageH;
		        trackBar1_ValueChanged(this, e);

				trackBar2.Value = (int)_currentSettingsWrapper.Settings.MaxDifH;
				trackBar3.Value = (int)(_currentSettingsWrapper.Settings.MinS * 100);
				trackBar4.Value = (int)(_currentSettingsWrapper.Settings.MaxS * 100);

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

			SettingsWrapper settingsWrapperToDelete = null;

			foreach (SettingsWrapper settings in SettingsList)
				if (settings.Name == name)
				{
					settingsWrapperToDelete = settings;
					break;
				}

			if (_currentSettingsWrapper == settingsWrapperToDelete)
				_currentSettingsWrapper = null;

			SettingsList.Remove(settingsWrapperToDelete);

		}

		public sealed class SettingsWrapper
		{
			public String Name { get; }
			public MarkerSettings Settings { get; }

			public SettingsWrapper(String name, MarkerSettings markerSettings)
			{
				Name = name;
				Settings = markerSettings;
			}
		}
	}
}
