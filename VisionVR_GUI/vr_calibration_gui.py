#!/usr/bin/env python3
"""
Vision VR Calibration GUI - Python Version
A configuration interface for the Vision VR Game
"""

import tkinter as tk
from tkinter import ttk, messagebox, scrolledtext
import json
import csv
import os
from pathlib import Path
from datetime import datetime
from typing import List, Dict, Any


class VRSettings:
    """Data class for VR game settings"""
    def __init__(self):
        self.gameDuration = 480.0  # in seconds
        self.focusY = 0.455
        self.focusScale = 0.381
        self.focusShape = 1  # 0=Circle, 1=Cross
        self.shapeDisplayDuration = 1000.0  # milliseconds
        self.betweenShapesDuration = 5000.0  # milliseconds
        self.focusChangeMode = 0  # 0=Static, 1=Fixed interval, 2=Random interval
        self.intervalSets = 1
        self.startingDistance = 1.0
        self.maxDistance = 10.0
        self.shapeScale = 0.05
        self.successRate = 80.0
        self.failRate = 20.0
        self.chunkSize = 15
        self.imageSets = []  # List of selected image sets
        
        # User details
        self.userID = ""
        self.trainingEye = 0  # 0=Right, 1=Left
        self.sessionTimestamp = ""
    
    def to_dict(self):
        """Convert settings to dictionary for JSON serialization"""
        return {
            'gameDuration': self.gameDuration,
            'focusY': self.focusY,
            'focusScale': self.focusScale,
            'focusShape': self.focusShape,
            'shapeDisplayDuration': self.shapeDisplayDuration,
            'betweenShapesDuration': self.betweenShapesDuration,
            'focusChangeMode': self.focusChangeMode,
            'intervalSets': self.intervalSets,
            'startingDistance': self.startingDistance,
            'maxDistance': self.maxDistance,
            'shapeScale': self.shapeScale,
            'successRate': self.successRate,
            'failRate': self.failRate,
            'chunkSize': self.chunkSize,
            'imageSets': self.imageSets,
            'userID': self.userID,
            'trainingEye': self.trainingEye,
            'sessionTimestamp': self.sessionTimestamp
        }
    
    @classmethod
    def from_dict(cls, data):
        """Create settings from dictionary"""
        settings = cls()
        for key, value in data.items():
            if hasattr(settings, key):
                setattr(settings, key, value)
        return settings


class CalibrationGUI:
    """Main GUI application for VR Calibration"""
    
    def __init__(self, root):
        self.root = root
        self.root.title("Vision VR - Calibration Interface")
        self.root.geometry("900x700")
        
        # Data folder
        self.data_folder = Path.home() / "VRUserData"
        self.config_folder = self.data_folder / "Configs"
        self.data_folder.mkdir(exist_ok=True)
        self.config_folder.mkdir(exist_ok=True)
        
        # Current settings
        self.settings = VRSettings()
        
        # Create notebook (tabbed interface)
        self.notebook = ttk.Notebook(root)
        self.notebook.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        # Create tabs
        self.create_settings_tab()
        self.create_user_tab()
        self.create_results_tab()
        
        # Load default settings if exists
        self.load_default_settings()
    
    def create_settings_tab(self):
        """Create the main settings configuration tab"""
        settings_frame = ttk.Frame(self.notebook)
        self.notebook.add(settings_frame, text="הגדרות (Settings)")
        
        # Create scrollable frame
        canvas = tk.Canvas(settings_frame)
        scrollbar = ttk.Scrollbar(settings_frame, orient="vertical", command=canvas.yview)
        scrollable_frame = ttk.Frame(canvas)
        
        scrollable_frame.bind(
            "<Configure>",
            lambda e: canvas.configure(scrollregion=canvas.bbox("all"))
        )
        
        canvas.create_window((0, 0), window=scrollable_frame, anchor="nw")
        canvas.configure(yscrollcommand=scrollbar.set)
        
        # Duration Settings
        duration_frame = ttk.LabelFrame(scrollable_frame, text="Duration Settings", padding=10)
        duration_frame.pack(fill=tk.X, padx=10, pady=5)
        
        ttk.Label(duration_frame, text="Test Duration (minutes):").grid(row=0, column=0, sticky=tk.W, pady=5)
        self.time_input = ttk.Entry(duration_frame, width=15)
        self.time_input.insert(0, "8")
        self.time_input.grid(row=0, column=1, pady=5)
        
        ttk.Label(duration_frame, text="Shape Display Duration (ms):").grid(row=1, column=0, sticky=tk.W, pady=5)
        self.shape_display_duration = ttk.Entry(duration_frame, width=15)
        self.shape_display_duration.insert(0, "1000")
        self.shape_display_duration.grid(row=1, column=1, pady=5)
        
        ttk.Label(duration_frame, text="Between Shapes Duration (ms):").grid(row=2, column=0, sticky=tk.W, pady=5)
        self.between_shapes_duration = ttk.Entry(duration_frame, width=15)
        self.between_shapes_duration.insert(0, "5000")
        self.between_shapes_duration.grid(row=2, column=1, pady=5)
        
        # Distance & Size Settings
        distance_frame = ttk.LabelFrame(scrollable_frame, text="Distance & Size Settings", padding=10)
        distance_frame.pack(fill=tk.X, padx=10, pady=5)
        
        ttk.Label(distance_frame, text="Starting Distance:").grid(row=0, column=0, sticky=tk.W, pady=5)
        self.starting_distance_slider = tk.Scale(distance_frame, from_=1, to=10, orient=tk.HORIZONTAL,
                                                  command=self.update_max_distance_range)
        self.starting_distance_slider.set(1)
        self.starting_distance_slider.grid(row=0, column=1, pady=5, sticky=tk.EW)
        self.starting_distance_value = ttk.Label(distance_frame, text="1")
        self.starting_distance_value.grid(row=0, column=2, pady=5)
        
        ttk.Label(distance_frame, text="Max Distance:").grid(row=1, column=0, sticky=tk.W, pady=5)
        self.max_distance_slider = tk.Scale(distance_frame, from_=1, to=10, orient=tk.HORIZONTAL)
        self.max_distance_slider.set(10)
        self.max_distance_slider.grid(row=1, column=1, pady=5, sticky=tk.EW)
        self.max_distance_value = ttk.Label(distance_frame, text="10")
        self.max_distance_value.grid(row=1, column=2, pady=5)
        
        ttk.Label(distance_frame, text="Shape Size:").grid(row=2, column=0, sticky=tk.W, pady=5)
        self.shape_size_slider = tk.Scale(distance_frame, from_=1, to=100, orient=tk.HORIZONTAL)
        self.shape_size_slider.set(10)
        self.shape_size_slider.grid(row=2, column=1, pady=5, sticky=tk.EW)
        self.shape_size_value = ttk.Label(distance_frame, text="10")
        self.shape_size_value.grid(row=2, column=2, pady=5)
        
        distance_frame.columnconfigure(1, weight=1)
        
        # Focus Point Settings
        focus_frame = ttk.LabelFrame(scrollable_frame, text="Focus Point Settings", padding=10)
        focus_frame.pack(fill=tk.X, padx=10, pady=5)
        
        ttk.Label(focus_frame, text="Focus Y Position:").grid(row=0, column=0, sticky=tk.W, pady=5)
        self.focus_y_slider = tk.Scale(focus_frame, from_=0, to=100, orient=tk.HORIZONTAL)
        self.focus_y_slider.set(45)
        self.focus_y_slider.grid(row=0, column=1, pady=5, sticky=tk.EW)
        self.focus_y_value = ttk.Label(focus_frame, text="45")
        self.focus_y_value.grid(row=0, column=2, pady=5)
        
        ttk.Label(focus_frame, text="Focus Scale:").grid(row=1, column=0, sticky=tk.W, pady=5)
        self.focus_scale_slider = tk.Scale(focus_frame, from_=0, to=100, orient=tk.HORIZONTAL)
        self.focus_scale_slider.set(38)
        self.focus_scale_slider.grid(row=1, column=1, pady=5, sticky=tk.EW)
        self.focus_scale_value = ttk.Label(focus_frame, text="38")
        self.focus_scale_value.grid(row=1, column=2, pady=5)
        
        ttk.Label(focus_frame, text="Focus Shape:").grid(row=2, column=0, sticky=tk.W, pady=5)
        self.focus_shape_dropdown = ttk.Combobox(focus_frame, values=["Circle", "Cross"], state="readonly", width=15)
        self.focus_shape_dropdown.current(1)
        self.focus_shape_dropdown.grid(row=2, column=1, pady=5, sticky=tk.W)
        
        ttk.Label(focus_frame, text="Focus Change Mode:").grid(row=3, column=0, sticky=tk.W, pady=5)
        self.focus_change_dropdown = ttk.Combobox(focus_frame, 
                                                   values=["Static", "Fixed Interval", "Random Interval"],
                                                   state="readonly", width=15)
        self.focus_change_dropdown.current(0)
        self.focus_change_dropdown.bind("<<ComboboxSelected>>", self.on_focus_change_mode_changed)
        self.focus_change_dropdown.grid(row=3, column=1, pady=5, sticky=tk.W)
        
        ttk.Label(focus_frame, text="Interval Sets:").grid(row=4, column=0, sticky=tk.W, pady=5)
        self.interval_sets_dropdown = ttk.Combobox(focus_frame, 
                                                    values=list(range(1, 11)), 
                                                    state="disabled", width=15)
        self.interval_sets_dropdown.current(0)
        self.interval_sets_dropdown.grid(row=4, column=1, pady=5, sticky=tk.W)
        
        focus_frame.columnconfigure(1, weight=1)
        
        # Success/Fail Rates & Chunk Size
        rate_frame = ttk.LabelFrame(scrollable_frame, text="Performance Settings", padding=10)
        rate_frame.pack(fill=tk.X, padx=10, pady=5)
        
        ttk.Label(rate_frame, text="Success Rate (%):").grid(row=0, column=0, sticky=tk.W, pady=5)
        self.success_rate_input = ttk.Entry(rate_frame, width=15)
        self.success_rate_input.insert(0, "80")
        self.success_rate_input.grid(row=0, column=1, pady=5)
        
        ttk.Label(rate_frame, text="Fail Rate (%):").grid(row=1, column=0, sticky=tk.W, pady=5)
        self.fail_rate_input = ttk.Entry(rate_frame, width=15)
        self.fail_rate_input.insert(0, "20")
        self.fail_rate_input.grid(row=1, column=1, pady=5)
        
        ttk.Label(rate_frame, text="Chunk Size:").grid(row=2, column=0, sticky=tk.W, pady=5)
        self.chunk_size_input = ttk.Entry(rate_frame, width=15)
        self.chunk_size_input.insert(0, "15")
        self.chunk_size_input.grid(row=2, column=1, pady=5)
        
        # Image Sets Selection
        image_frame = ttk.LabelFrame(scrollable_frame, text="Image Sets", padding=10)
        image_frame.pack(fill=tk.X, padx=10, pady=5)
        
        self.image_set_vars = []
        for i in range(10):
            var = tk.BooleanVar()
            chk = ttk.Checkbutton(image_frame, text=f"Image Set {i+1}", variable=var)
            chk.grid(row=i//2, column=i%2, sticky=tk.W, pady=2)
            self.image_set_vars.append(var)
        
        # Buttons
        button_frame = ttk.Frame(scrollable_frame)
        button_frame.pack(fill=tk.X, padx=10, pady=10)
        
        ttk.Button(button_frame, text="Save Configuration", 
                  command=self.save_configuration).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="Load Configuration", 
                  command=self.load_configuration).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="Continue to User Details", 
                  command=self.goto_user_tab).pack(side=tk.RIGHT, padx=5)
        
        canvas.pack(side="left", fill="both", expand=True)
        scrollbar.pack(side="right", fill="y")
        
        # Bind slider updates
        self.starting_distance_slider.configure(command=lambda v: self.update_slider_display())
        self.max_distance_slider.configure(command=lambda v: self.update_slider_display())
        self.shape_size_slider.configure(command=lambda v: self.update_slider_display())
        self.focus_y_slider.configure(command=lambda v: self.update_slider_display())
        self.focus_scale_slider.configure(command=lambda v: self.update_slider_display())
    
    def create_user_tab(self):
        """Create the user details tab"""
        user_frame = ttk.Frame(self.notebook)
        self.notebook.add(user_frame, text="פרטי משתמש (User Details)")
        
        # Center frame
        center_frame = ttk.Frame(user_frame)
        center_frame.place(relx=0.5, rely=0.5, anchor=tk.CENTER)
        
        info_frame = ttk.LabelFrame(center_frame, text="User Information", padding=20)
        info_frame.pack(padx=20, pady=20)
        
        ttk.Label(info_frame, text="Name:").grid(row=0, column=0, sticky=tk.W, pady=10, padx=5)
        self.name_input = ttk.Entry(info_frame, width=30)
        self.name_input.grid(row=0, column=1, pady=10, padx=5)
        
        ttk.Label(info_frame, text="ID:").grid(row=1, column=0, sticky=tk.W, pady=10, padx=5)
        self.id_input = ttk.Entry(info_frame, width=30)
        self.id_input.grid(row=1, column=1, pady=10, padx=5)
        
        ttk.Label(info_frame, text="Age:").grid(row=2, column=0, sticky=tk.W, pady=10, padx=5)
        self.age_input = ttk.Entry(info_frame, width=30)
        self.age_input.grid(row=2, column=1, pady=10, padx=5)
        
        ttk.Label(info_frame, text="Gender:").grid(row=3, column=0, sticky=tk.W, pady=10, padx=5)
        self.gender_dropdown = ttk.Combobox(info_frame, values=["Male", "Female"], state="readonly", width=28)
        self.gender_dropdown.current(0)
        self.gender_dropdown.grid(row=3, column=1, pady=10, padx=5)
        
        # Birth Date
        ttk.Label(info_frame, text="Birth Date:").grid(row=4, column=0, sticky=tk.W, pady=10, padx=5)
        date_frame = ttk.Frame(info_frame)
        date_frame.grid(row=4, column=1, pady=10, padx=5, sticky=tk.W)
        
        current_year = datetime.now().year
        self.year_dropdown = ttk.Combobox(date_frame, values=list(range(current_year, 1900, -1)), 
                                          state="readonly", width=8)
        self.year_dropdown.current(0)
        self.year_dropdown.pack(side=tk.LEFT, padx=2)
        
        self.month_dropdown = ttk.Combobox(date_frame, values=list(range(1, 13)), 
                                           state="readonly", width=5)
        self.month_dropdown.current(0)
        self.month_dropdown.pack(side=tk.LEFT, padx=2)
        
        self.day_dropdown = ttk.Combobox(date_frame, values=list(range(1, 32)), 
                                         state="readonly", width=5)
        self.day_dropdown.current(0)
        self.day_dropdown.pack(side=tk.LEFT, padx=2)
        
        ttk.Label(info_frame, text="Training Eye:").grid(row=5, column=0, sticky=tk.W, pady=10, padx=5)
        self.eye_dropdown = ttk.Combobox(info_frame, values=["Right", "Left"], state="readonly", width=28)
        self.eye_dropdown.current(0)
        self.eye_dropdown.grid(row=5, column=1, pady=10, padx=5)
        
        # Buttons
        button_frame = ttk.Frame(info_frame)
        button_frame.grid(row=6, column=0, columnspan=2, pady=20)
        
        ttk.Button(button_frame, text="← Previous", 
                  command=self.goto_settings_tab).pack(side=tk.LEFT, padx=10)
        ttk.Button(button_frame, text="Start Session", 
                  command=self.save_and_start).pack(side=tk.LEFT, padx=10)
    
    def create_results_tab(self):
        """Create the results viewing tab"""
        results_frame = ttk.Frame(self.notebook)
        self.notebook.add(results_frame, text="תוצאות (Results)")
        
        # Results list frame
        list_frame = ttk.LabelFrame(results_frame, text="Session Results", padding=10)
        list_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        # Treeview for results
        columns = ("Timestamp", "User ID", "Eye", "Accuracy", "Trials")
        self.results_tree = ttk.Treeview(list_frame, columns=columns, show="headings", height=15)
        
        for col in columns:
            self.results_tree.heading(col, text=col)
            self.results_tree.column(col, width=150)
        
        scrollbar = ttk.Scrollbar(list_frame, orient=tk.VERTICAL, command=self.results_tree.yview)
        self.results_tree.configure(yscroll=scrollbar.set)
        
        self.results_tree.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        # Buttons
        button_frame = ttk.Frame(results_frame)
        button_frame.pack(fill=tk.X, padx=10, pady=5)
        
        ttk.Button(button_frame, text="Refresh Results", 
                  command=self.load_results).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="View Details", 
                  command=self.view_result_details).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="Delete Selected", 
                  command=self.delete_result).pack(side=tk.LEFT, padx=5)
        
        # Load results on creation
        self.load_results()
    
    def update_slider_display(self):
        """Update slider value displays"""
        self.starting_distance_value.config(text=str(self.starting_distance_slider.get()))
        self.max_distance_value.config(text=str(self.max_distance_slider.get()))
        self.shape_size_value.config(text=str(self.shape_size_slider.get()))
        self.focus_y_value.config(text=str(self.focus_y_slider.get()))
        self.focus_scale_value.config(text=str(self.focus_scale_slider.get()))
        self.update_max_distance_range()
    
    def update_max_distance_range(self, event=None):
        """Update max distance slider range based on starting distance"""
        start_dist = self.starting_distance_slider.get()
        self.max_distance_slider.configure(from_=start_dist)
        if self.max_distance_slider.get() < start_dist:
            self.max_distance_slider.set(start_dist)
        self.update_slider_display()
    
    def on_focus_change_mode_changed(self, event=None):
        """Enable/disable interval sets based on focus change mode"""
        mode = self.focus_change_dropdown.current()
        if mode == 1:  # Fixed interval
            self.interval_sets_dropdown.config(state="readonly")
        else:
            self.interval_sets_dropdown.config(state="disabled")
    
    def goto_user_tab(self):
        """Navigate to user details tab"""
        # Validate that at least one image set is selected
        if not any(var.get() for var in self.image_set_vars):
            messagebox.showwarning("Warning", "Please select at least one image set")
            return
        self.notebook.select(1)
    
    def goto_settings_tab(self):
        """Navigate to settings tab"""
        self.notebook.select(0)
    
    def save_configuration(self):
        """Save current configuration to a file"""
        # Create save dialog
        dialog = tk.Toplevel(self.root)
        dialog.title("Save Configuration")
        dialog.geometry("400x150")
        dialog.transient(self.root)
        dialog.grab_set()
        
        ttk.Label(dialog, text="Configuration Name:").pack(pady=10)
        
        name_var = tk.StringVar(value=datetime.now().strftime("%Y-%m-%d_%H-%M-%S"))
        name_entry = ttk.Entry(dialog, textvariable=name_var, width=40)
        name_entry.pack(pady=5)
        name_entry.select_range(0, tk.END)
        name_entry.focus()
        
        def do_save():
            config_name = name_var.get().strip()
            if not config_name:
                messagebox.showwarning("Warning", "Please enter a configuration name")
                return
            
            # Collect settings
            settings = self.collect_settings()
            
            # Save to file
            config_path = self.config_folder / f"{config_name}.json"
            with open(config_path, 'w') as f:
                json.dump(settings.to_dict(), f, indent=2)
            
            messagebox.showinfo("Success", "Configuration saved successfully")
            dialog.destroy()
        
        button_frame = ttk.Frame(dialog)
        button_frame.pack(pady=20)
        
        ttk.Button(button_frame, text="Save", command=do_save).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="Cancel", command=dialog.destroy).pack(side=tk.LEFT, padx=5)
    
    def load_configuration(self):
        """Load a saved configuration"""
        # Get list of configs
        config_files = list(self.config_folder.glob("*.json"))
        
        if not config_files:
            messagebox.showinfo("Info", "No saved configurations found")
            return
        
        # Create load dialog
        dialog = tk.Toplevel(self.root)
        dialog.title("Load Configuration")
        dialog.geometry("500x400")
        dialog.transient(self.root)
        dialog.grab_set()
        
        ttk.Label(dialog, text="Select Configuration:").pack(pady=10)
        
        # Listbox for configs
        listbox_frame = ttk.Frame(dialog)
        listbox_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=5)
        
        listbox = tk.Listbox(listbox_frame, height=15)
        scrollbar = ttk.Scrollbar(listbox_frame, orient=tk.VERTICAL, command=listbox.yview)
        listbox.configure(yscrollcommand=scrollbar.set)
        
        for config_file in sorted(config_files, key=lambda x: x.stat().st_mtime, reverse=True):
            listbox.insert(tk.END, config_file.stem)
        
        listbox.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        def do_load():
            selection = listbox.curselection()
            if not selection:
                messagebox.showwarning("Warning", "Please select a configuration")
                return
            
            config_name = listbox.get(selection[0])
            config_path = self.config_folder / f"{config_name}.json"
            
            try:
                with open(config_path, 'r') as f:
                    data = json.load(f)
                
                settings = VRSettings.from_dict(data)
                self.apply_settings(settings)
                
                messagebox.showinfo("Success", "Configuration loaded successfully")
                dialog.destroy()
            except Exception as e:
                messagebox.showerror("Error", f"Failed to load configuration: {str(e)}")
        
        def do_delete():
            selection = listbox.curselection()
            if not selection:
                messagebox.showwarning("Warning", "Please select a configuration")
                return
            
            if messagebox.askyesno("Confirm", "Are you sure you want to delete this configuration?"):
                config_name = listbox.get(selection[0])
                config_path = self.config_folder / f"{config_name}.json"
                config_path.unlink()
                listbox.delete(selection[0])
                messagebox.showinfo("Success", "Configuration deleted")
        
        button_frame = ttk.Frame(dialog)
        button_frame.pack(pady=10)
        
        ttk.Button(button_frame, text="Load", command=do_load).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="Delete", command=do_delete).pack(side=tk.LEFT, padx=5)
        ttk.Button(button_frame, text="Cancel", command=dialog.destroy).pack(side=tk.LEFT, padx=5)
    
    def collect_settings(self):
        """Collect current settings from UI"""
        settings = VRSettings()
        
        # Durations
        try:
            settings.gameDuration = float(self.time_input.get()) * 60  # Convert to seconds
        except ValueError:
            settings.gameDuration = 480.0
        
        try:
            settings.shapeDisplayDuration = float(self.shape_display_duration.get())
        except ValueError:
            settings.shapeDisplayDuration = 1000.0
        
        try:
            settings.betweenShapesDuration = float(self.between_shapes_duration.get())
        except ValueError:
            settings.betweenShapesDuration = 5000.0
        
        # Distances and sizes
        settings.startingDistance = float(self.starting_distance_slider.get())
        settings.maxDistance = float(self.max_distance_slider.get())
        settings.shapeScale = float(self.shape_size_slider.get()) * 0.005
        
        # Focus settings
        settings.focusY = float(self.focus_y_slider.get()) / 100.0
        settings.focusScale = float(self.focus_scale_slider.get()) / 100.0
        settings.focusShape = self.focus_shape_dropdown.current()
        settings.focusChangeMode = self.focus_change_dropdown.current()
        settings.intervalSets = int(self.interval_sets_dropdown.get()) if self.interval_sets_dropdown.get() else 1
        
        # Rates and chunk size
        try:
            settings.successRate = float(self.success_rate_input.get())
        except ValueError:
            settings.successRate = 80.0
        
        try:
            settings.failRate = float(self.fail_rate_input.get())
        except ValueError:
            settings.failRate = 20.0
        
        try:
            settings.chunkSize = int(self.chunk_size_input.get())
        except ValueError:
            settings.chunkSize = 15
        
        # Image sets
        settings.imageSets = [i+1 for i, var in enumerate(self.image_set_vars) if var.get()]
        
        return settings
    
    def apply_settings(self, settings: VRSettings):
        """Apply settings to UI"""
        # Durations
        self.time_input.delete(0, tk.END)
        self.time_input.insert(0, str(settings.gameDuration / 60))
        
        self.shape_display_duration.delete(0, tk.END)
        self.shape_display_duration.insert(0, str(settings.shapeDisplayDuration))
        
        self.between_shapes_duration.delete(0, tk.END)
        self.between_shapes_duration.insert(0, str(settings.betweenShapesDuration))
        
        # Sliders
        self.starting_distance_slider.set(settings.startingDistance)
        self.max_distance_slider.set(settings.maxDistance)
        self.shape_size_slider.set(settings.shapeScale / 0.005)
        self.focus_y_slider.set(settings.focusY * 100)
        self.focus_scale_slider.set(settings.focusScale * 100)
        
        # Dropdowns
        self.focus_shape_dropdown.current(settings.focusShape)
        self.focus_change_dropdown.current(settings.focusChangeMode)
        self.interval_sets_dropdown.set(str(settings.intervalSets))
        
        # Rates
        self.success_rate_input.delete(0, tk.END)
        self.success_rate_input.insert(0, str(settings.successRate))
        
        self.fail_rate_input.delete(0, tk.END)
        self.fail_rate_input.insert(0, str(settings.failRate))
        
        self.chunk_size_input.delete(0, tk.END)
        self.chunk_size_input.insert(0, str(settings.chunkSize))
        
        # Image sets
        for i, var in enumerate(self.image_set_vars):
            var.set((i+1) in settings.imageSets)
        
        self.update_slider_display()
        self.on_focus_change_mode_changed()
    
    def load_default_settings(self):
        """Load default settings from vr_settings.json if exists"""
        settings_path = self.data_folder / "vr_settings.json"
        if settings_path.exists():
            try:
                with open(settings_path, 'r') as f:
                    data = json.load(f)
                settings = VRSettings.from_dict(data)
                self.apply_settings(settings)
            except Exception:
                pass
    
    def save_and_start(self):
        """Save settings and user details, then close"""
        # Validate user ID
        user_id = self.id_input.get().strip()
        if not user_id:
            messagebox.showwarning("Warning", "Please enter a user ID")
            return
        
        # Collect settings
        settings = self.collect_settings()
        
        # Add user details
        settings.userID = user_id
        settings.trainingEye = self.eye_dropdown.current()
        settings.sessionTimestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        
        # Save settings to vr_settings.json
        settings_path = self.data_folder / "vr_settings.json"
        with open(settings_path, 'w') as f:
            json.dump(settings.to_dict(), f, indent=2)
        
        # Save user details to CSV
        self.save_user_details(settings)
        
        messagebox.showinfo("Success", 
                          "Settings saved successfully!\n\n"
                          f"Settings file: {settings_path}\n"
                          "You can now start the VR game.")
        
        # Optionally close the application
        # self.root.quit()
    
    def save_user_details(self, settings: VRSettings):
        """Save user details to CSV"""
        csv_path = self.data_folder / "user_details.csv"
        
        user_name = self.name_input.get().strip() or "N/A"
        user_id = settings.userID
        
        try:
            age = int(self.age_input.get())
        except ValueError:
            age = -1
        
        gender = self.gender_dropdown.get()
        birth_year = int(self.year_dropdown.get())
        birth_month = int(self.month_dropdown.get())
        birth_day = int(self.day_dropdown.get())
        eye_text = self.eye_dropdown.get()
        timestamp = settings.sessionTimestamp
        
        # Check if user exists
        user_exists = False
        if csv_path.exists():
            with open(csv_path, 'r', newline='', encoding='utf-8') as f:
                reader = csv.DictReader(f)
                rows = list(reader)
            
            for i, row in enumerate(rows):
                if row['ID'] == user_id:
                    # Update existing user
                    previous_eye = row['EyeTrained']
                    if previous_eye == "Both" or previous_eye == eye_text:
                        new_eye = previous_eye
                    else:
                        new_eye = "Both"
                    
                    rows[i] = {
                        'ID': user_id,
                        'Name': user_name,
                        'Age': age if age != -1 else "N/A",
                        'Gender': gender,
                        'BirthYear': birth_year,
                        'BirthMonth': birth_month,
                        'BirthDay': birth_day,
                        'EyeTrained': new_eye,
                        'FirstAdded': row['FirstAdded'],
                        'LastUpdate': timestamp
                    }
                    user_exists = True
                    break
            
            if user_exists:
                # Write back
                with open(csv_path, 'w', newline='', encoding='utf-8') as f:
                    fieldnames = ['ID', 'Name', 'Age', 'Gender', 'BirthYear', 'BirthMonth', 
                                'BirthDay', 'EyeTrained', 'FirstAdded', 'LastUpdate']
                    writer = csv.DictWriter(f, fieldnames=fieldnames)
                    writer.writeheader()
                    writer.writerows(rows)
                return
        
        # Add new user
        file_exists = csv_path.exists()
        with open(csv_path, 'a', newline='', encoding='utf-8') as f:
            fieldnames = ['ID', 'Name', 'Age', 'Gender', 'BirthYear', 'BirthMonth', 
                        'BirthDay', 'EyeTrained', 'FirstAdded', 'LastUpdate']
            writer = csv.DictWriter(f, fieldnames=fieldnames)
            
            if not file_exists:
                writer.writeheader()
            
            writer.writerow({
                'ID': user_id,
                'Name': user_name,
                'Age': age if age != -1 else "N/A",
                'Gender': gender,
                'BirthYear': birth_year,
                'BirthMonth': birth_month,
                'BirthDay': birth_day,
                'EyeTrained': eye_text,
                'FirstAdded': timestamp,
                'LastUpdate': timestamp
            })
    
    def load_results(self):
        """Load game results from CSV"""
        # Clear existing items
        for item in self.results_tree.get_children():
            self.results_tree.delete(item)
        
        csv_path = self.data_folder / "game_results.csv"
        if not csv_path.exists():
            return
        
        try:
            with open(csv_path, 'r', newline='', encoding='utf-8') as f:
                reader = csv.DictReader(f)
                for row in reader:
                    self.results_tree.insert('', 0, values=(
                        row.get('Timestamp', ''),
                        row.get('UserID', ''),
                        row.get('EyeTrained', ''),
                        row.get('OverallAccuracy', ''),
                        row.get('OverallTrials', '')
                    ))
        except Exception as e:
            messagebox.showerror("Error", f"Failed to load results: {str(e)}")
    
    def view_result_details(self):
        """View detailed results for selected session"""
        selection = self.results_tree.selection()
        if not selection:
            messagebox.showwarning("Warning", "Please select a result to view")
            return
        
        item = self.results_tree.item(selection[0])
        values = item['values']
        
        # Create detail dialog
        dialog = tk.Toplevel(self.root)
        dialog.title("Session Results Details")
        dialog.geometry("600x500")
        dialog.transient(self.root)
        
        text = scrolledtext.ScrolledText(dialog, wrap=tk.WORD, width=70, height=30)
        text.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        # Display results
        text.insert(tk.END, f"Timestamp: {values[0]}\n")
        text.insert(tk.END, f"User ID: {values[1]}\n")
        text.insert(tk.END, f"Eye Trained: {values[2]}\n")
        text.insert(tk.END, f"Accuracy: {values[3]}\n")
        text.insert(tk.END, f"Total Trials: {values[4]}\n")
        text.insert(tk.END, "\n" + "="*60 + "\n\n")
        text.insert(tk.END, "For detailed level-by-level results,\n")
        text.insert(tk.END, "please check the game_results.csv file.\n")
        
        text.config(state=tk.DISABLED)
        
        ttk.Button(dialog, text="Close", command=dialog.destroy).pack(pady=10)
    
    def delete_result(self):
        """Delete selected result from CSV"""
        selection = self.results_tree.selection()
        if not selection:
            messagebox.showwarning("Warning", "Please select a result to delete")
            return
        
        if not messagebox.askyesno("Confirm", "Are you sure you want to delete this result?"):
            return
        
        item = self.results_tree.item(selection[0])
        timestamp = item['values'][0]
        
        csv_path = self.data_folder / "game_results.csv"
        
        try:
            with open(csv_path, 'r', newline='', encoding='utf-8') as f:
                reader = csv.DictReader(f)
                rows = [row for row in reader if row.get('Timestamp') != timestamp]
            
            with open(csv_path, 'w', newline='', encoding='utf-8') as f:
                if rows:
                    fieldnames = rows[0].keys()
                    writer = csv.DictWriter(f, fieldnames=fieldnames)
                    writer.writeheader()
                    writer.writerows(rows)
            
            self.load_results()
            messagebox.showinfo("Success", "Result deleted successfully")
        except Exception as e:
            messagebox.showerror("Error", f"Failed to delete result: {str(e)}")


def main():
    """Main entry point"""
    root = tk.Tk()
    app = CalibrationGUI(root)
    root.mainloop()


if __name__ == "__main__":
    main()
