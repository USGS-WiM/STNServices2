'''
Created on Jul 17, 2017

@author: gpetrochenkov
'''
#!/usr/bin/env python3
import sys
import csv
current_path = sys.path[0]
sys.path.append(''.join([current_path,'\\..']))
sys.path.append(''.join([current_path,'\\..\\netCDF_Utils']))
import pandas as pd
from netCDF4 import Dataset
import numpy as np
from pytz import timezone
from csv_readers import Leveltroll, MeasureSysLogger, House, Hobo, RBRSolo, Waveguage
import unit_conversion as uc
import argparse
from datetime import datetime
import json



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
        csv_dict['Exceptions'].append('Trouble reading csv file, check for correct type')
        csv_dict['Status'] = 'fFailed'
        return 1
   
    
    data = {}
    data['time']  = instrument.utc_millisecond_data[::4].tolist()
    data['pressure'] = (instrument.pressure_data[::4] / uc.PSI_TO_DBAR).tolist()
    
    idx = inputs['out_filename'].rfind('\\')
  
    with open(''.join([inputs['out_filename'][:idx+1],'data.json']), 'w') as outfile:
        json.dump(data, outfile)
        
    csv_dict['File Created'].append('data.json')
    
    return None

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

def check_file_type(file_name, pressure_type='Sea Pressure'):
    index = file_name.rfind('.')
    if index == -1:
        csv_file = open(file_name)
        try:
            dialect = csv.Sniffer().sniff(csv_file.read(1024))
            return True
        except:
            if pressure_type == 'Air Pressure':
                try:
                    ds = Dataset(file_name, 'r')
                    return True
                except:
                    return False
            return False
    else: 
        if file_name[index:] == '.csv':
            return True
        else:
            if pressure_type == 'Air Pressure':
                if file_name[index:] == '.nc':
                    return True
                
            return False
    
def process_file(args, csv_dict):
    daylight_savings = False
    if args['daylight_savings'].lower() == 'true':
        daylight_savings = True
        
    inputs = {
        'in_filename' : args['in_fname'],
        'out_filename' : args['out_fname'],
        'creator_name' : 'test',
        'creator_email' : 'test',
        'creator_url' : 'test',
        'instrument_name' : args['instrument_name'],
        'stn_station_number': 'test',
        'stn_instrument_id': 'test',
        'latitude' : 0,
        'longitude' : 0,
        'tz_info' : args['tz_info'],
        'daylight_savings': daylight_savings,
        'datum': args['datum'],
        'initial_sensor_orifice_elevation': 0,
        'final_sensor_orifice_elevation': 0,
        'salinity' : 'test',
        'initial_land_surface_elevation': 0,
        'final_land_surface_elevation': 0,
        'deployment_time' : '20160101 1200',
        'retrieval_time' : '20160101 1201',
        'sea_name' : 'test',    
        'pressure_type' : args['pressure_type'],
        }
     
    #checks for the correct file type
    if check_file_type(inputs['in_filename'], inputs['pressure_type']) == False:
        csv_dict['Exceptions'].append('Not proper file type')
        csv_dict['Status'] = 'failed'
        return (2, None)
       
    try:
        data_issues = convert_to_netcdf(inputs, csv_dict)
        if data_issues is None:
            csv_dict['Status'] = 'complete with no errors'
            return (7, None)
        else:
            csv_dict['Status'] = 'failed'
            return (5, None)
    except:
        csv_dict['Exceptions'].append('General Script Error')
        csv_dict['Status'] = 'failed'
        return (5, None)
     
    


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
     
     
    parser.add_argument('in_fname',
                        help='directory location of raw sea pressure csv file')
    parser.add_argument('out_fname',
                        help='directory location to output netCDF file')
    parser.add_argument('pressure_type',
                        help='type of pressure data to preview')
    parser.add_argument('daylight_savings',
                        help='daylight savings')
    parser.add_argument('tz_info', 
                        help='Timezone')
    parser.add_argument('datum',
                        help='geospatial vertical reference')
    parser.add_argument('instrument_name',
                        help='Instrument Name')
       
       
       
    args = vars(parser.parse_args(sys.argv[1:]))
    
    
    def output_csv(csv_dict, output):
        for x in csv_dict:
            csv_dict[x] = pd.Series(csv_dict[x])
        df = pd.DataFrame(csv_dict)
        idx = output.rfind('.')
        df.to_csv(path_or_buf=''.join([output[:idx], '.csv']))
        
             
    def create_dict():
        csv_dict = {}
        csv_dict['Status'] = 'pending'
        csv_dict['File Created'] = []
        csv_dict['Exceptions'] = []
        csv_dict['Warnings'] = []
        csv_dict['Script start date'] = datetime.strftime(datetime.now(), '%m-%d-%Y %H:%M:%S')
        csv_dict['Script Version'] = 'V 1.1'
        
        return csv_dict
        
    
    args['in_fname'] = args['in_fname']
    args['out_fname'] = ''.join([args['out_fname'], '_chop_preview.nc'])
    args['pressure_type'] = args['pressure_type']
    args['instrument_name'] = args['instrument_name']
    args['stn_station_number'] = 'Test'
    args['stn_instrument_id'] = 'Test'
    args['latitude'] = 0
    args['longitude'] = 0
    args['initial_sensor_orifice_elevation'] = 0
    args['final_sensor_orifice_elevation'] = 0
    
    
    csv_dict = create_dict()
    process_file(args, csv_dict)
    

    csv_dict['Script end date'] = datetime.strftime(datetime.now(), '%m-%d-%Y %H:%M:%S')
    
    output_csv(csv_dict, args['out_fname'])
    
    sys.exit()
    
    
    
