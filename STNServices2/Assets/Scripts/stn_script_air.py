'''
Created on Jul 17, 2017

@author: gpetrochenkov
'''
#!/usr/bin/env python3
import sys
import csv
import pandas as pd
current_path = sys.path[0]
sys.path.append(''.join([current_path,'\\..']))
sys.path.append(''.join([current_path,'\\..\\netCDF_Utils']))
from netCDF4 import Dataset
import numpy as np
import netCDF_Utils.nc as nc
from pytz import timezone
from csv_readers import Leveltroll, MeasureSysLogger, House, Hobo, RBRSolo, Waveguage
import unit_conversion as uc
import argparse
from tools.storm_options import StormOptions
from tools.storm_graph import StormGraph, Bool
from datetime import datetime

INSTRUMENTS = {
    'Level Troll': Leveltroll,
    'RBR Solo': RBRSolo,
    'Wave Guage': Waveguage,
    'USGS Homemade': House,
    'Measurement Specialties': MeasureSysLogger,
    'Hobo': Hobo }

def convert_to_netcdf(inputs, csv_dict):
    translated = translate_inputs(inputs)
    instrument = INSTRUMENTS[translated['instrument_name']]()
    instrument.user_data_start_flag = 0
    for key in translated:
        setattr(instrument, key, translated[key])
        
    try:
        instrument.read()
       
    except:
        csv_dict['Exceptions'].append('Trouble reading csv file. Check for correct type')
        return None
       
    try:
        instrument.write(pressure_type=translated['pressure_type'])
        csv_dict['File Created'].append(instrument.out_filename)
    except:
        csv_dict['Exceptions'].append('Trouble writing netCDF file. Check that all inputs are valid')
        
    
    return instrument.bad_data


DATATYPES = {
    'latitude': np.float32,
    'longitude': np.float32,
    'initial_water_depth': np.float32,
    'final_water_depth': np.float32,
    'device_depth': np.float32,
    'tzinfo': timezone,
    'sea_pressure' : bool }

def translate(inputs):
    translated = translate_inputs(inputs)
    instrument = INSTRUMENTS[translated['instrument_name']]()
    instrument.user_data_start_flag = 0
    for key in translated:
        setattr(instrument, key, translated[key])

def translate_inputs(inputs):
    translated = dict()
    for key in inputs: # cast everything to the right type
        if key in DATATYPES:
            translated[key] = DATATYPES[key](inputs[key])
        else:
            translated[key] = inputs[key]
    return translated

def find_index(array, value):
    
    array = np.array(array)
    idx = (np.abs(array-value)).argmin()
    
    return idx

def check_file_type(file_name):
    index = file_name.rfind('.')
    if index == -1:
        csv_file = open(file_name)
        try:
            dialect = csv.Sniffer().sniff(csv_file.read(1024))
            return True
        except:
            return False
    else: 
        if file_name[index:] == '.csv':
            return True
        else:
            return False
    
def process_file(args, csv_dict):
    daylight_savings = False
    if args['daylight_savings'].lower() == 'true':
        daylight_savings = True
        
    inputs = {
        'in_filename' : args['in_fname'],
        'out_filename' : args['out_fname'],
        'creator_name' : args['creator_name'],
        'creator_email' : args['creator_email'],
        'creator_url' : args['creator_url'],
        'instrument_name' : args['instrument_name'],
        'stn_station_number': args['stn_station_number'],
        'stn_instrument_id': args['stn_instrument_id'],
        'good_start_date': args['good_start_date'],
        'good_end_date': args['good_end_date'],
        'latitude' : args['latitude'],
        'longitude' : args['longitude'],
        'tz_info' : args['tz_info'],
        'daylight_savings': daylight_savings,
        'datum': args['datum'],
        'pressure_type' : args['pressure_type'],
        'initial_sensor_orifice_elevation': args['initial_sensor_orifice_elevation'],
        'final_sensor_orifice_elevation': args['final_sensor_orifice_elevation'],
        }
     
    #checks for the correct file type
    if check_file_type(inputs['in_filename']) == False:
        csv_dict['Exceptions'].append('Not a csv file')
        return (2, None)
       
    #check for dates in chronological order if sea pressure file
    if inputs['pressure_type'] == 'Sea Pressure':
        inputs['deployment_time'] = uc.datestring_to_ms(inputs['deployment_time'], '%Y%m%d %H%M', \
                                                             inputs['tz_info'],
                                                             inputs['daylight_savings'])
        
        inputs['retrieval_time'] = uc.datestring_to_ms(inputs['retrieval_time'], '%Y%m%d %H%M', \
                                                             inputs['tz_info'],
                                                             inputs['daylight_savings'])
        if inputs['retrieval_time'] <= inputs['deployment_time']:
            csv_dict['Exceptions'].append('Retrieval/deployment dates are not in chronological order')
            return (3, None)
        
    
    try:
        data_issues = convert_to_netcdf(inputs, csv_dict)
        
        if data_issues is None:
            return (7, None)
    except:
        csv_dict['Exceptions'].append('General Script Error')
        return (5, None)
     
    time = nc.get_time(inputs['out_filename'])
    
    start_index = find_index(time,uc.datestring_to_ms(inputs['good_start_date'], '%Y%m%d %H%M', \
                                                         inputs['tz_info'],
                                                         inputs['daylight_savings']))
    end_index = find_index(time,uc.datestring_to_ms(inputs['good_end_date'], '%Y%m%d %H%M', \
                                                         inputs['tz_info'],
            
                                                         inputs['daylight_savings']))
    #checks for chronological order of dates
    if end_index <= start_index:
        csv_dict['Exceptions'].append('Good start dates are not in chronolgical order')
        return (3, None)
       
    air_pressure = False
    if args['pressure_type'] == 'Air Pressure':
        air_pressure = True
         
    try:
        nc.chop_netcdf(inputs['out_filename'], ''.join([inputs['out_filename'],'chop.nc']), 
                           start_index, end_index, air_pressure)
    except:
        csv_dict['Exceptions'].append('Bad Good Start and/or End Date')
        return (8, None)
  
    
    if data_issues:
        return (1, ''.join([inputs['out_filename'],'chop.nc']))
        csv_dict['Warnings'].append('Some data did not pass the QAQC tests.  The data was sliced for only valid data')
    else:
        return (0, ''.join([inputs['out_filename'],'chop.nc']))
    #will be either 0 for perfect overlap or 1 for slicing some data
    

def make_baro_graph(args, csv_dict): 
    
    so = StormOptions()
    
    so.air_fname = args['air_fname']
    
    
    so.format_output_fname(args['out_fname'])
    so.timezone = args['tz_info']
    so.daylight_savings = args['daylight_savings']
    
    if 'baro_y_min' in args and args['baro_y_min'] is not None:
        so.baroYLims = []
        so.baroYLims.append(args['baro_y_min'])
        so.baroYLims.append(args['baro_y_max'])
        
    if 'wl_y_min' in args and args['wl_y_min'] is not None:
        so.wlYLims = []
        so.wlYLims.append(args['wl_y_min'])
        so.wlYLims.append(args['wl_y_max'])
        
    try:   
        sg = StormGraph()
        so.graph['Storm Tide with Wind Data'] = Bool(False)
        so.graph['Storm Tide with Unfiltered Water Level'] = Bool(False)
        so.graph['Storm Tide Water Level'] = Bool(False)
        so.graph['Atmospheric Pressure'] = Bool(True)
        sg.process_graphs(so)
        csv_dict['File Created'].append(so.output_fname + '_barometric_pressure.png')
        
        return 0
    except:
        csv_dict['Exceptions'].append('Could not create barometric pressure visualizations')
        return 4
       


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    
    
    parser.add_argument('air_fname',
                        help='directory location of raw air pressure csv file')
    parser.add_argument('out_fname',
                        help='directory location to output netCDF file')
    parser.add_argument('creator_name',
                        help='name of user running the script')
    parser.add_argument('creator_email',
                        help='email of user running the script')
    parser.add_argument('creator_url',
                        help='url of organization the user running the script belongs to')
    parser.add_argument('air_instrument_name',
                        help='name of the instrument used to measure pressure')
    parser.add_argument('air_stn_instrument_id',
                        help='STN Instrument ID')
    parser.add_argument('air_stn_station_number',
                        help='STN Site ID')
    parser.add_argument('air_latitude', type=float,
                        help='latitude of instrument')
    parser.add_argument('air_longitude', type=float,
                        help='longitude of instrument')
    parser.add_argument('tz_info', 
                        help='time zone of instrument output dates')
    parser.add_argument('daylight_savings', 
                        help='if time zone is in daylight savings')
    parser.add_argument('datum', 
                        help='geospatial vertical reference point')
    parser.add_argument('air_initial_sensor_orifice_elevation', type=float,
                        help='tape down to sensor at deployment time')
    parser.add_argument('air_final_sensor_orifice_elevation', type=float,
                        help='tape down to sensor at retrieval time')
    parser.add_argument('air_good_start_date',
                        help='first date for chopping the time series')
    parser.add_argument('air_good_end_date',
                        help='last date for chopping the time series')
    
      
    args = vars(parser.parse_args(sys.argv[1:]))


    output = args['out_fname']
    
    args['in_fname'] = args['air_fname'] 
    args['out_fname'] = ''.join([args['out_fname'], '_air.nc'])
    args['pressure_type'] = 'Air Pressure'
    args['instrument_name'] = args['air_instrument_name']
    args['stn_station_number'] = args['air_stn_station_number']
    args['stn_instrument_id'] = args['air_stn_instrument_id']
    args['latitude'] = args['air_latitude']
    args['longitude'] = args['air_longitude']
    args['initial_sensor_orifice_elevation'] = args['air_initial_sensor_orifice_elevation']
    args['final_sensor_orifice_elevation'] = args['air_final_sensor_orifice_elevation']
    args['good_start_date'] = args['air_good_start_date']
    args['good_end_date'] = args['air_good_end_date']
    
    csv_dict = {}
    csv_dict['File Created'] = []
    csv_dict['Exceptions'] = []
    csv_dict['Warnings'] = []
    csv_dict['Status'] = 'pending'
    csv_dict['Script start date'] = datetime.strftime(datetime.now(), '%m-%d-%Y %H:%M:%S')
    csv_dict['Script Version'] = 'v 1.2'
    
    
    csv_dict['Data Start Date/time'] = datetime.strftime(datetime.strptime(args['good_start_date'],'%Y%m%d %H%M'), '%m-%d-%Y %H:%M:%S')
    csv_dict['Data End Date/time'] = datetime.strftime(datetime.strptime(args['good_end_date'],'%Y%m%d %H%M'), '%m-%d-%Y %H:%M:%S')
    
    air_code, args['air_fname'] = process_file(args, csv_dict)
    
    
    code = make_baro_graph(args, csv_dict)
    
    csv_dict['Script end date'] = datetime.strftime(datetime.now(), '%m-%d-%Y %H:%M:%S')
    if (air_code == 0 or air_code == 1) and (code == 0 or code==1):
        csv_dict['Status'] = 'complete with no errors'
    else:
        csv_dict['Status'] = 'failed'
    
    for x in csv_dict:
        csv_dict[x] = pd.Series(csv_dict[x])
    df = pd.DataFrame(csv_dict)
    df.to_csv(path_or_buf=''.join([output, '.csv']))
     
    sys.exit()
        
    
   
    
